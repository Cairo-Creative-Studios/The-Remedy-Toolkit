using UnityEngine;

namespace Remedy.Cameras
{
    [SchematicComponent("Cameras/Third Person")]
    [ExecuteAlways]
    public class ThirdPersonCamera : CameraOperator
    {
        public SignalData AimPosition;

        [Tooltip("The Object to Follow.")]
        public Vector3 FollowOffset;
        public float HorizontalFollowSpeed = 10f;
        public float VerticalFollowSpeed = 3f;
        [Tooltip("The Camera sticks to the ground beneath the follow target until this distance is reached, then this is used as an offset from the Follow Target")]
        public float YDistanceAllowance = 3f;
        public Vector3 GroundedOffset = new Vector3(0, 1, 0);
        [Tooltip("Max distance from the Followed Object.")]
        public float MaxDistance = 5f;

        public float LevelAngle = 5f;
        public float AutoLevelDelay = 1f;
        public float AutoLevelSpeed = 5f;

        [Tooltip("An angular force applied to distance the Camera Rotation away from walls over time.")]
        public float OptimalDistancePush = 1f;
        public float OptimalDistance = 10f;

        public Rigidbody Rigidbody;
        public float LookaheadOrientationStrength = 1f;
        [Tooltip("The Velocity (downward) at which the Rigidbody must be moving to fully forcefully orient the camera down.")]
        public float LookdownOrientationMaxVelocity = 10f;
        [Tooltip("The maximum angle downward the to orient the camera when falling")]
        public float LookdownOrientationMaxAngle = 40f;
        [Tooltip("The Velocity (downward) at which the Rigidbody must be moving to fully forcefully orient the camera down.")]
        public float LookupOrientationMaxVelocity = 10f;
        [Tooltip("The maximum angle downward the to orient the camera when falling")]
        public float LookupOrientationMaxAngle = 30f;
        public float LookdownOrientationSpeed = 10f;

        [Tooltip("What is collided with to move the Camera closer to the Player")]
        public LayerMask CollisionLayers;
        public float CollisionReactSpeed = 10f;
        public float YOffsetTransitionSpeed = 10f;

        private float _targetScale = 1f;
        private float _currentScale = 1f;
        private RaycastHit _floorHit = default;
        private RaycastHit _wallHit = default;
        private Vector3 _allowanceOffset = Vector3.zero;
        private Vector3 _horPos = Vector3.zero;
        private Vector3 _vertPos = Vector3.zero;
        private bool _lookingDown = false;
        private float _currentYOffset;
        private float _lastFloorHit;


        private void Update()
        {
            if (Follow == null) return;

            if(Application.isPlaying)
            {
                if (Physics.Raycast(Camera.position, Camera.forward, out RaycastHit aimHit, Mathf.Infinity))
                {
                    AimPosition?.Invoke(aimHit.point);
                }
            }

            var prevRootPos = Root.position;
            transform.position = Root.position;
            Root.position = prevRootPos;

            _allowanceOffset.y = -YDistanceAllowance;

            _horPos.x = Mathf.Lerp(Root.position.x, Follow.position.x + FollowOffset.x, HorizontalFollowSpeed * Time.deltaTime);
            _horPos.z = Mathf.Lerp(Root.position.z, Follow.position.z + FollowOffset.z, HorizontalFollowSpeed * Time.deltaTime);

            // TODO: Apply an Offset Grace Period (Transition between the two different offsets so the camera doesn't jump up and down)
            /*        if (Physics.SphereCast(Follow.position + Vector3.up, 0.5f, Vector3.down, out _floorHit, YDistanceAllowance, CollisionLayers))
                    {
                        _vertPos.y = Mathf.Lerp(Root.position.y, _floorHit.point.y + GroundedOffset.y, VerticalFollowSpeed * Time.deltaTime);
                    }
                    else
                    {
                        _vertPos.y = Mathf.Lerp(Root.position.y, Follow.position.y + FollowOffset.y, VerticalFollowSpeed * Time.deltaTime);
                    }*/

            // TODO: Apply an Offset Grace Period (Transition between the two different offsets so the camera doesn't jump up and down)
            if (Physics.SphereCast(Follow.position + Vector3.up, 0.5f, Vector3.down, out _floorHit, YDistanceAllowance, CollisionLayers))
            {
                _currentYOffset = Mathf.Lerp(_currentYOffset, 0f, YOffsetTransitionSpeed * Time.deltaTime);
                _lastFloorHit = _floorHit.point.y + GroundedOffset.y;
            }
            else
            {
                _currentYOffset = Mathf.Lerp(_currentYOffset, 1f, YOffsetTransitionSpeed * Time.deltaTime);
            }

            float goalY = Mathf.Lerp(_lastFloorHit, Follow.position.y + FollowOffset.y, _currentYOffset);
            _vertPos.y = Mathf.Lerp(Root.position.y, goalY, VerticalFollowSpeed * Time.deltaTime);

            Root.position = _horPos + _vertPos;

            // Rotation smoothing
            _actualXAxis = Mathf.Lerp(_actualXAxis, _xAxis, SmoothSpeed * Time.deltaTime);

            // Level look axis angle
            _timeSinceInput += Time.deltaTime;
            if (_timeSinceInput > AutoLevelDelay)
            {
                if (!_lookingDown)
                {
                    // Reset camera to resting angle
                    if (CurrentVolume != null && CurrentVolume.ChangeRestingAngle)
                    {
                        _actualYAxis = Mathf.Lerp(_actualYAxis, CurrentVolume.RestingAngle, AutoLevelSpeed * Time.deltaTime);
                        _yAxis = Mathf.Lerp(_yAxis, CurrentVolume.RestingAngle, AutoLevelSpeed * Time.deltaTime);
                    }
                    else
                    {
                        _actualYAxis = Mathf.Lerp(_actualYAxis, LevelAngle, AutoLevelSpeed * Time.deltaTime);
                        _yAxis = Mathf.Lerp(_yAxis, LevelAngle, AutoLevelSpeed * Time.deltaTime);
                    }
                }

                // Lookahead in the direction of the Rigidbody Horizontal Velocity
                if (Rigidbody != null && (Mathf.Abs(Rigidbody.linearVelocity.x) > 1 || Mathf.Abs(Rigidbody.linearVelocity.z) > 1))
                {
                    _actualXAxis = Mathf.LerpAngle(_actualXAxis, Rigidbody.transform.eulerAngles.y, LookaheadOrientationStrength * Time.deltaTime);
                    _xAxis = Mathf.LerpAngle(_xAxis, Rigidbody.transform.eulerAngles.y, LookaheadOrientationStrength * Time.deltaTime);
                }


                // Orient relative to walls
                if (Physics.SphereCast(Follow.position, 0.25f, Follow.right, out _wallHit, OptimalDistance, CollisionLayers))
                {
                    Vector3 projectedForward = Vector3.ProjectOnPlane(Follow.right, _wallHit.normal).normalized;
                    Quaternion targetYaw = Quaternion.LookRotation(projectedForward, Vector3.up);
                    var euler = targetYaw.eulerAngles.y;

                    if (Mathf.Abs(Mathf.DeltaAngle(euler, Root.eulerAngles.y)) < 60)
                        _xAxis = Mathf.LerpAngle(_xAxis, euler, OptimalDistancePush * Time.deltaTime);
                }
                else if (Physics.SphereCast(Follow.position, 0.25f, -Follow.right, out _wallHit, OptimalDistance, CollisionLayers))
                {
                    Vector3 projectedForward = Vector3.ProjectOnPlane(-Follow.right, _wallHit.normal).normalized;
                    Quaternion targetYaw = Quaternion.LookRotation(projectedForward, Vector3.up);
                    var euler = targetYaw.eulerAngles.y;

                    if (Mathf.Abs(Mathf.DeltaAngle(euler, Root.eulerAngles.y)) < 100)
                        _xAxis = Mathf.LerpAngle(_xAxis, euler, OptimalDistancePush * Time.deltaTime);
                }

            }
            else
            {
                _actualYAxis = Mathf.Lerp(_actualYAxis, _yAxis, SmoothSpeed * Time.deltaTime);
            }

            // Aim down when falling
            if (!Physics.SphereCast(Follow.position + Vector3.up, 0.5f, Vector3.down, out _floorHit, YDistanceAllowance, CollisionLayers))
            {
                _lookingDown = true;

                if (Rigidbody.linearVelocity.y <= 0)
                {
                    float t = Mathf.Clamp(Mathf.Abs(Rigidbody.linearVelocity.y) / LookdownOrientationMaxVelocity, 0.2f, 1f);

                    _yAxis = Mathf.Lerp(_yAxis, Mathf.Lerp(0, LookdownOrientationMaxAngle, t), LookdownOrientationSpeed * Time.deltaTime);
                    _actualYAxis = Mathf.Lerp(_actualYAxis, Mathf.Lerp(0, LookdownOrientationMaxAngle, t), LookdownOrientationSpeed * Time.deltaTime);
                }
                else if (Rigidbody.linearVelocity.y > 0)
                {
                    float t = Mathf.Clamp(Mathf.Abs(Rigidbody.linearVelocity.y) / LookupOrientationMaxVelocity, 0f, 1f);

                    _yAxis = Mathf.Lerp(_yAxis, Mathf.Lerp(0, -LookupOrientationMaxAngle, t), LookdownOrientationSpeed * Time.deltaTime);
                    _actualYAxis = Mathf.Lerp(_actualYAxis, Mathf.Lerp(0, -LookupOrientationMaxAngle, t), LookdownOrientationSpeed * Time.deltaTime);
                }
            }
            else
                _lookingDown = false;

            // Apply rotation to Root
            Root.rotation = Quaternion.Euler(_actualYAxis, _actualXAxis, 0f);

            // SphereCast from Root to max range in the local -Z direction
            Vector3 origin = Root.position;
            Vector3 direction = -Root.forward;
            Vector3 targetCameraPos = origin + direction * MaxDistance;

            float targetDistance;
            if (CurrentVolume != null && CurrentVolume.ChangeZoom)
                targetDistance = CurrentVolume.Zoom;
            else
                targetDistance = MaxDistance;

            if (Physics.SphereCast(origin, CameraCollisionRadius, direction, out RaycastHit hit, targetDistance, CollisionLayers, QueryTriggerInteraction.Ignore))
            {
                // Reduce distance based on hit
                float hitDistance = Mathf.Clamp(hit.distance, 0.5f, targetDistance);
                _targetScale = hitDistance;
            }
            else
            {
                _targetScale = targetDistance;
            }

            // Smooth scale interpolation
            _currentScale = Mathf.Lerp(_currentScale, _targetScale, CollisionReactSpeed * Time.deltaTime);

            Root.localScale = new Vector3(1, 1, _currentScale);


            var dir = Root.position - Camera.position;
            Camera.rotation = Quaternion.Slerp(Camera.rotation, Quaternion.LookRotation(dir, Vector3.up), 20f * Time.deltaTime);

            Camera.localPosition = Vector3.Lerp(Camera.localPosition, new Vector3(0, 0, -1), 20f * Time.deltaTime);


            UpdateAimDirection();
        }

        private void OnTriggerEnter(Collider other)
        {
            var propVolume = other.GetComponent<CameraPropertiesVolume>();
            if (propVolume != null)
            {
                CurrentVolume = propVolume;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var propVolume = other.GetComponent<CameraPropertiesVolume>();
            if (propVolume == CurrentVolume)
            {
                CurrentVolume = null;
            }
        }

    }

}