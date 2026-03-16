using Remedy.Schematics.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

[SchematicComponent("Movement/3D/Character Motion Context")]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(CharacterRaycastContext))]
public class CharacterMotionContext : MonoBehaviour
{
    // IO
    [InputSignal]
    [Parameter("Value", typeof(bool))]
    public Signal<bool> JumpInput = new("JumpInput");
    [InputSignal]
    [Parameter("Value", typeof(Vector2))]
    public Signal<Vector2> MoveInput = new("MoveInput");
    [InputSignal]
    [Parameter("Amount", typeof(float))]
    public Signal<float> StaminaUpdated = new("StaminaUpdated");
    [InputSignal]
    [Parameter("Direction", typeof(Vector3))]
    public Signal<Vector3> SetDirectionOfGravity = new("SetDirectionOfGravity");

    [OutputSignal]
    public Signal OnJump = new("OnJump");
    [OutputSignal]
    public Signal<Vector3> OnMoveOnGround = new("OnMove");
    [OutputSignal]
    public Signal OnFall = new("OnFall");

    //Data
    [SchematicProperties]
    public CharacterMotionProperties Properties;

    // C# Events
    public Action<Vector3> OnPushed;
    public Action SpringPathFinished;
    
    // Accessors
    public Transform CameraTransform { get; private set; }
    public bool IsKinematic = true;
    public Vector3 Velocity => _velocity;
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public Vector3 HorizontalDirection => _horizontalMovementDirection;
    public bool JumpPooled => _jumpPooled;
    public float CurrentStamina => _currentStamina;
    public Vector3 MoveDirection => _moveInput;

    // cache
    [SerializeField]
    [HideInInspector]
    private bool _cached;
    [SerializeField, HideInInspector]
    private Transform _transform;
    [SerializeField, HideInInspector]
    private Rigidbody _rb;
    private CharacterRaycastContext _raycastContext;
    [SerializeField, HideInInspector]
    private SphereCollider _collider;
    [SerializeField, HideInInspector]
    private Transform _gravReference;

    // state vars
    private Vector3 _worldDirection = default;
    private Vector3 _velocity;
    private Vector3 _horizontalVelocity;
    private Vector3 _wallProjectedVelocity;
    private Vector3 _wallHitPoint = default;
    private Vector3 _horizontalMovementDirection = default;

    private Vector3[] _forces = new Vector3[10];
    private Vector3 _overrideVelocity = default;
    private int _forcesCount = 0;
    private float _dt = 0;
    private Vector3 _vec3Zero;
    private bool _jumpPooled = false;
    private bool _physicsStep = false;
    private float _currentStamina;
    private bool _canBePushed = false;
    
    private List<Vector3> _springPath;
    private int _springPathIndex = 0; 
    private float _springStartY;
    private bool _isResting;

    private Vector3 _moveInput =  default;
    
    // Lifecycle
    private void OnEnable()
    {
        _cached = false;
        Cache();

        MoveInput.Subscribe(this, (Vector2 val) =>
        {
            _moveInput.x = val.x;
            _moveInput.z = val.y;
        });

        CameraTransform = CameraSubsystem.Camera?.transform;

        if (CameraTransform == null && Camera.main != null)
        {
            CameraTransform = Camera.main.transform;
            Debug.LogWarning($"MotionContext: Global camera not found, defaulting to MainCamera ({CameraTransform.name})");
        }

        if (CameraTransform == null)
            Debug.LogError("MotionContext: No camera found for motions to reference!");

        StaminaUpdated?.Subscribe(this, (float value) => _currentStamina = value);

        SetDirectionOfGravity?.Subscribe(this, (Vector3 val) =>
        {
            _worldDirection = val;
        });
    }

    private void OnDisable()
    {
        StaminaUpdated?.Unsubscribe(this);
        MoveInput.Unsubscribe(this);
        SetDirectionOfGravity.Unsubscribe(this);
    }

    private void FixedUpdate()
    {
        _physicsStep = !_physicsStep;
        _rb.isKinematic = IsKinematic;

        if(true)
        {
            _raycastContext.PerformGroundCast();

            // calculate spring arc (if there is one)
            if (_springPath != null )
            { 
                if(_springPathIndex < _springPath.Count)
                {
                    var point = _springPath[_springPathIndex];

                    SpringTo(_springStartY, point, Properties.SpringStrength, Properties.SpringDamper, Properties.SpringMaxForce);

                    if (Vector3.Distance(_transform.position, point) < Properties.SpringEpsilon)
                    {
                        _springPathIndex++;
                        SpringPathFinished?.Invoke();
                    }
                }
                else
                {
                    _springPath = null;
                    _springPathIndex = 0;
                }
            }

            HandleWallSlidingMovement();

            // apply forces
            for (int i = 0; i < _forcesCount; i++)
            {
                _velocity += _forces[i];
            }

            if (_overrideVelocity != _vec3Zero)
            {
                _velocity = _overrideVelocity;
                _overrideVelocity = _vec3Zero;
            }
            if (_wallHitPoint != _vec3Zero)
            {
                _velocity.x = _wallProjectedVelocity.x;
                _velocity.z = _wallProjectedVelocity.z;
            }

            _horizontalVelocity.x = _velocity.x;
            _horizontalVelocity.z = _velocity.z;

            // rest
            _isResting = false;

            if (_horizontalVelocity.sqrMagnitude < Properties.MovementSnapThreshold) // *gringe for the magic numbers*
            {
                if(Mathf.Pow(Mathf.Abs(_velocity.y) * 10, 2) < Properties.MovementSnapThreshold)
                {
                    _isResting = true;
                }
            }
            else
            {
                _horizontalMovementDirection = _horizontalVelocity.normalized;
            }
        }
        // apply velocity
        if (!_isResting)
        {
            _rb.WakeUp();

            // velocity overrides
            if (_velocity.y < 0 && _raycastContext.GroundCastResult.TryGetClosestHit(out var hit))
            {
                if (hit.distance < Properties.MinDistanceFromGround)
                {
                    //_velocity.y = 0;
                }
            }

            _rb.linearVelocity = _velocity;

            OnMoveOnGround?.Invoke(this, _horizontalVelocity);
        }
        else
        {
            _rb.Sleep();
            _wallProjectedVelocity = _vec3Zero;
        }
/*        else
        {*/
            // reset velocity
            //_velocity = _rb.linearVelocity;

            // reset forces
            _forcesCount = 0;
        //}

        for (int i = 0; i < _forces.Length; i++)
        {
            _forces[i] = _vec3Zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(_canBePushed && collision.relativeVelocity.sqrMagnitude > Properties.CollisionPushVelocityThreshold)
        {
            OnPushed?.Invoke(collision.relativeVelocity);
            _velocity += collision.relativeVelocity * Properties.CollisionPushInfluence;
        }
    }

    // public api

    /// <summary>
    /// Moves the Character to a specific position and forces the Rigidbody to Kinematic so that it can set the exact position of the Character without forces acting on it.
    /// </summary>
    /// <param name="position">The position to move to.</param>
    /// <param name="canBePushed">Whether collisions from other objects can switch the Rigidbody out of Kinematic mode.</param>
    public void SetPositionLocked(Vector3 position, bool canBePushed = true)
    {
        _rb.isKinematic = true;
        _rb.position = position;
        _canBePushed = canBePushed;
    }

    public void SetRotationLocked(Quaternion rotation)
    {
        bool isKinematic = _rb.isKinematic;

        _rb.isKinematic = true;
        _rb.rotation = rotation;
        _rb.isKinematic = isKinematic;
    }

    /// <summary>
    /// Applies the given force to the Rigidbody
    /// </summary>
    /// <param name="force"></param>
    /// <returns></returns>
    public void ApplyForce(Vector3 force, bool overrideOtherForces = false)
    {
        if (!overrideOtherForces)
        {
            if (_forcesCount < _forces.Length)
            {
                force = RotateByGravityDirection(force);
                _forces[_forcesCount] = force;

                _forcesCount++;
            }
        }
        else
        {
            force = RotateByGravityDirection(force);
            _overrideVelocity = force;
        }
    }

    public void ApplyHorizontalForce(Vector3 force, bool overrideOtherForces = false)
    {
        if (!overrideOtherForces)
        {
            if (_forcesCount < _forces.Length)
            {
                force.y = 0;
                force = RotateByGravityDirection(force);
                _forces[_forcesCount] = force;

                _forcesCount++;
            }
        }
        else
        {
            force.y = 0;
            force = RotateByGravityDirection(force);

            _overrideVelocity = force;
        }
    }

    public void ApplyVerticalForce(float force, bool overrideOtherForces = false)
    {
        if (!overrideOtherForces)
        {
            if (_forcesCount < _forces.Length)
            {
                var appliedForce = _forces[_forcesCount];
                appliedForce = _vec3Zero;
                appliedForce.y = force;
                appliedForce = RotateByGravityDirection(appliedForce);
                _forces[_forcesCount] = appliedForce;

                _forcesCount++;
            }
        }
        else
        {
            var appliedForce = _vec3Zero;
            appliedForce.y = force;
            appliedForce = RotateByGravityDirection(appliedForce);

            _overrideVelocity = appliedForce;
        }
    }

    public Vector3 PredictNextPosition()
    {
        return _rb.position + _rb.linearVelocity * _dt * _dt;
    }

    public void ApplyGravity()
    {
        _velocity += RotateByGravityDirection(Physics.gravity * _dt);
    }

    /// <summary>
    /// Uses physics forces to push the character in a direction at a specific speed
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="maxSpeed"></param>
    /// <param name="acceleration"></param>
    /// <param name="decceleration"></param>
    public void ApplyCappedHorizontalForce(Vector3 direction, float maxSpeed, float acceleration, float decceleration, bool applyCameraTransform = true)
    {
        float desiredSpeed = maxSpeed;
        float desiredRateOfChange = acceleration;

        if (Mathf.Abs(direction.x) + Mathf.Abs(direction.z) < 0.1f)
        {
            desiredSpeed = 0;
            desiredRateOfChange = decceleration;
        }

        if (applyCameraTransform && CameraTransform != null)
            direction = TransformInputDirection(direction);

        direction.y = 0f;
        direction.Normalize();

        Vector3 desiredVel = (direction * desiredSpeed);

        Vector3 clampedVel = desiredVel.normalized * desiredSpeed;
        Vector3 excess = HorizontalVelocity - RotateByGravityDirection(clampedVel);

        Vector3 correction = -excess.normalized * desiredRateOfChange;

        ApplyHorizontalForce(correction * _dt);
    }

    /// <summary>
    /// Iterates through an array of positions to spring the rigidbody in a path
    /// </summary>
    /// <param name="startPositionY"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public void FollowSpringPath(float startPositionY, List<Vector3> path)
    {
        _springStartY = startPositionY;
        _springPathIndex = 0;
        _springPath = path;
    }

    /// <summary>
    /// Springs the Rigidbody toward the given position with the given force
    /// </summary>
    /// <param name="groundYPosition"></param>
    /// <param name="position"></param>
    /// <param name="strength"></param>
    /// <param name="damper"></param>
    /// <param name="maxForce"></param>
    public void SpringTo(float groundYPosition, Vector3 position, float strength, float damper, float maxForce)
    {
        SpringHorizontally(position, strength, damper, maxForce);
        SpringToVertically(groundYPosition, position.y, strength, damper, maxForce);
    }

    /// <summary>
    /// Springs the Rigidbody to the given HorizontalPosition
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="strength"></param>
    /// <param name="damper"></param>
    /// <param name="maxForce"></param>
    public void SpringHorizontally(Vector3 targetPosition, float strength, float damper, float maxForce)
    {
        Vector3 horizontalPosition = _transform.position;
        horizontalPosition.y = 0;
        Vector3 delta = targetPosition - horizontalPosition;
        Vector3 springForce = (delta * strength) - (_horizontalVelocity * damper);
        springForce = Vector3.ClampMagnitude(springForce * _dt, maxForce);
        ApplyHorizontalForce(springForce);
    }

    /// <summary>
    /// Springs the Rigidbody to the given vertical position
    /// </summary>
    /// <param name="groundYPosition"></param>
    /// <param name="targetYPosition"></param>
    /// <param name="strength"></param>
    /// <param name="damper"></param>
    /// <param name="maxForce"></param>
    public void SpringToVertically(float groundYPosition, float targetYPosition, float strength, float damper, float maxForce)
    {
        float heightError =  (groundYPosition + targetYPosition) - _transform.position.y;
        float differenceForce = ((heightError * strength) - (Velocity.y * damper));

        ApplyVerticalForce(differenceForce);
    }

    public void TeleportToPosition(Vector3 position)
    {
        _rb.isKinematic = true;
        _rb.position = position;
        _rb.isKinematic = false;
    }
    
    public void PoolJump()
    {
        _jumpPooled = true;
    }

    public void PerformJump()
    {
        _jumpPooled = false;
    }

    public Vector3 TransformInputDirection(Vector3 inputDirection)
    {
        return CameraTransform.TransformDirection(inputDirection);
    }

    public void WallCheckInMoveDirection()
    {
        _moveInput = TransformInputDirection(_moveInput);
        _raycastContext.PerformWallCast(_moveInput);
    }

    // internals
    private void Cache()
    {
        if(!_cached)
        {
            _dt = Time.fixedDeltaTime;

            _transform = gameObject.GetComponent<Transform>();
            _rb = gameObject.GetComponent<Rigidbody>();
            _raycastContext = gameObject.GetComponent<CharacterRaycastContext>();
            _collider = gameObject.GetComponent<SphereCollider>();

            if(_gravReference == null)
            {
                if(transform.childCount > 0)
                    _gravReference = transform.GetChild(0);
                else
                    _gravReference = new GameObject("Gravity Reference").transform;

                _gravReference.parent = _transform;
            }

            if (Properties != null)
                _collider.sharedMaterial = Properties.PhysicsMaterial;

            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.useGravity = false;

            _cached = true;
        }
    }

    private void HandleWallSlidingMovement()
    {
        if (!Properties.SlideOnWall) return;

        WallCheckInMoveDirection();

        if (_raycastContext.WallCastResult.TryGetClosestHit(out var closestWallHit))
        {
            _wallHitPoint = closestWallHit.point;
            _wallProjectedVelocity = Vector3.ProjectOnPlane(_horizontalVelocity, -closestWallHit.normal);

            if (_wallProjectedVelocity.magnitude > 0.01f)
            {
                _raycastContext.PerformWallCast(_wallProjectedVelocity.normalized);
                if (_raycastContext.WallCastResult.HasHit)
                {
                    _wallProjectedVelocity = _vec3Zero;
                }
            }
        }
        else
        {
            _wallProjectedVelocity = default;
            _wallHitPoint = default;
        }
    }

    Vector3 RotateByGravityDirection(Vector3 inputForce)
    {
        if (_worldDirection == _vec3Zero) 
            _worldDirection = Vector3.forward;

        _gravReference.rotation = Quaternion.LookRotation(_worldDirection, Vector3.forward);
        var result = _gravReference.TransformDirection(inputForce);
        return result;
    }

    private void OnDrawGizmos()
    {
        Cache();

        if(!_rb.isKinematic)
            Gizmos.color = new Color(0f, 0.3f, 1f, 0.75f);
        else
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.75f);

        Gizmos.DrawSphere(_transform.position + _collider.center, _collider.radius);

        for (int i = 0; i < _forcesCount; i++)
        {
            if (_forces[i] != Vector3.zero)
                CustomGizmos.DrawArrow(_transform.position, _forces[i], _forces[i].magnitude * 2, new Color(1f, 1f, 0f, 0.3f));
        }

        if (Properties != null && Properties.SlideOnWall && _raycastContext.WallCastResult.TryGetClosestHit(out var closestWallHit))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_transform.position, _raycastContext.WallPosition);
            CustomGizmos.DrawArrow(_raycastContext.WallPosition, _wallProjectedVelocity, _rb.linearVelocity.magnitude, new Color(0, 1f, 0, 0.5f));
        }
        else
        if (_rb.linearVelocity != Vector3.zero)
            CustomGizmos.DrawArrow(_transform.position, _rb.linearVelocity, _rb.linearVelocity.magnitude * 0.5f, new Color(0, 1f, 0, 0.5f));

        CustomGizmos.DrawArrow(_transform.position, _moveInput, 2f, new Color(1, 1f, 1, 0.5f));
    }
}