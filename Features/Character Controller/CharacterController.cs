using Remedy.Framework;
using Remedy.Schematics.Utils;
using UnityEngine;

[SchematicComponent("Movement/3D/Default Character Controller")]
[RequireComponent(typeof(CharacterMotionContext))]
[RequireComponent(typeof(CharacterRaycastContext))]
public class PhysicsBasedCharacterController : SchematicMonoBehavior
{
    [SchematicEventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.MoveInput))]
    public Signal<Vector2> MoveInput => _motionContext.MoveInput;
    [SchematicEventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.JumpInput))]
    public Signal<bool> JumpInput => _motionContext.JumpInput;
    [InputSignal]
    [Parameter("Position", typeof(Vector3))]
    public Signal<Vector3> MoveToPosition = new("MoveToPosition");

    [SchematicProperties]
    public CharacterControllerProperties Properties;

    // Cached
    private bool _cached = false;
    private CharacterRaycastContext _raycastContext;
    private CharacterMotionContext _motionContext;
    private float _dt;

    // State vars
    private Vector3 _moveInput = default;
    private bool _prevGrounded = false;
    private bool _isJumping = false;
    private float _jumpResetTime = 0f;
    private float _jumpBufferTime = 0f;
    private float _jumpArcY = 0f;
    private bool _reachedJumpArc = false;
    private float _tempHangTimeDuration = -1; // > 0 = use this instead of default Hang Time
    private float _currentHangTime = 0f;
    private float _jumpStartGroundHeight;
    private float _groundY = 0;
    private bool _hadJumped = false;
    private bool _canJump = true;
    private bool _enableGravity = true;

    private void OnEnable()
    {
        _cached = false;
        Cache();

        _motionContext.IsKinematic = false;
        _canJump = true;

        _currentHangTime = Properties.HangTimeDuration;
        _jumpBufferTime = Properties.JumpBuffer;

        MoveInput.Subscribe(this, (Vector2 value) =>
        {
            _moveInput.x = value.x;
            _moveInput.z = value.y;
        });

        JumpInput.Subscribe(this, (bool value) =>
        {
            _moveInput.y = value ? 1 : 0;

            if (value)
            {
                if (_canJump)
                {
                    _jumpBufferTime = 0f;
                    _canJump = false;
                }
            }
            else
                _canJump = true;
        });
    }

    private void Cache()
    {
        if(!_cached)
        {
            _raycastContext = gameObject.GetComponent<CharacterRaycastContext>();
            _motionContext = gameObject.GetComponent<CharacterMotionContext>();
            _dt = Time.fixedDeltaTime;
            _cached = true;
        }
    }

    private void OnDisable()
    {
        MoveInput.Unsubscribe(this);
        JumpInput.Unsubscribe(this);
        MoveToPosition.Unsubscribe(this);
    }

    private void FixedUpdate()
    {
        HandleGroundRide();
        _motionContext.ApplyCappedHorizontalForce(_moveInput, Properties.MoveSpeed, Properties.Acceleration, Properties.Deceleration);
        HandleJump();
    }

    private void HandleGroundRide()
    {
        if (_raycastContext.IsGrounded && !_isJumping)
        {
            _hadJumped = false;
            _groundY = _raycastContext.GroundPosition.y;
            _prevGrounded = true;
            _reachedJumpArc = false;
            _motionContext.SpringToVertically(_groundY, Properties.RideHeight, Properties.RideSpringStrength, Properties.HangTimeDamper, Properties.MaxHoverForce);
        }
        else
        {
            float hangTimeDuration = _tempHangTimeDuration > 0 ? _tempHangTimeDuration : Properties.HangTimeDuration;

            if (_enableGravity && !(_currentHangTime < hangTimeDuration && _hadJumped) && !_raycastContext.IsGrounded)
                _motionContext.ApplyGravity();

            _prevGrounded = false;
        }
    }

    private void HandleJump()
    {
        float hangTimeDuration = _tempHangTimeDuration > 0 ? _tempHangTimeDuration : Properties.HangTimeDuration;

        if (_moveInput.y == 0)
        {
            if(_prevGrounded)
                _jumpResetTime += _dt;

            if(_isJumping)
            {
                _jumpBufferTime = Properties.JumpBuffer;
                _isJumping = false;
            }
        }

        if (!_isJumping && _prevGrounded)
        {
            _jumpResetTime += _dt;

            if (_jumpResetTime > Properties.JumpResetDelay && _jumpBufferTime < Properties.JumpBuffer)
            {
                _isJumping = true;
                _prevGrounded = false;
                _jumpResetTime = 0f;
                _jumpArcY = _raycastContext.GroundPosition.y + Properties.JumpHeight;

                if(_motionContext.Velocity.y < Properties.JumpSpeed)
                    _motionContext.ApplyVerticalForce(Properties.JumpSpeed);

                _jumpStartGroundHeight = _raycastContext.GroundPosition.y;
                _reachedJumpArc = false;

                _hadJumped = true;
                _enableGravity = false;
            }
        }
        else if(_isJumping)
        {
            Vector3 predictedPos = _motionContext.PredictNextPosition();

            if (predictedPos.y < _jumpArcY)
            {
                if (_motionContext.Velocity.y < Properties.JumpSpeed)
                    _motionContext.ApplyVerticalForce(Properties.JumpSpeed);
            }
            else
            {
                _isJumping = false;
                _currentHangTime = 0f;
                _reachedJumpArc = true;
            }
        }
        else if (_currentHangTime < hangTimeDuration && _hadJumped)
        {
            if(_reachedJumpArc)
                _motionContext.SpringToVertically(_jumpStartGroundHeight, Properties.JumpHeight, Properties.RideSpringStrength, Properties.HangTimeDamper, Properties.MaxHoverForce);

            _currentHangTime += _dt;
        }
        else if(!_raycastContext.IsGrounded)
        {
            _motionContext.ApplyVerticalForce(-Properties.FallSpeed);
            _enableGravity = true;
        }

        _jumpBufferTime += _dt;
    }


    private void OnCollisionEnter(Collision collision)
    {
        foreach (var contact in collision.contacts)
        {
            if (Vector3.Dot(contact.normal, Vector3.down) > 0.5f)
            {
                _currentHangTime = Properties.HangTimeDuration; 
                break;
            }
        }
    }
}
