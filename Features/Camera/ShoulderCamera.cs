using UnityEngine;
//using SaintsField.Playa;

namespace Remedy.Cameras
{
    [SchematicComponent("Cameras/Shoulder")]
    [ExecuteAlways]
    public class ShoulderCamera : CameraOperator
    {
        [Parameter("Direction", typeof(Vector3))]
        public SignalData ActualLookDirection;
        public SignalData GoalLookDirection;
        public SignalData AimPosition;

        public enum Shoulders
        {
            Left,
            Right
        }

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Follow")]*/
        [Tooltip("The shoulder the Camera is 'peering over'")]
        public Shoulders Shoulder = Shoulders.Left;
        [Tooltip("The distance offset from the character's shoulder.")]
        public Vector3 ShoulderOffset = new Vector3(0.5f, 0f, 0f);
        public float ShoulderSwitchSpeed = 10f;
        public bool AutoSwitch = true;
        public float SwitchDistance = 2f;
        public Vector3 FollowOffset;
        public float HorizontalFollowSpeed = 10f;
        public float VerticalFollowSpeed = 3f;
        [Tooltip("The Camera sticks to the ground beneath the follow target until this distance is reached, then this is used as an offset from the Follow Target")]
        public float YDistanceAllowance = 3f;
        [Tooltip("Max distance from the Followed Object.")]
        public float MaxDistance = 5f;


        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Optimizations")]*/
        [Tooltip("An angular force applied to distance the Camera Rotation away from walls over time.")]
        public float OptimalDistancePush = 1f;
        public float OptimalDistance = 10f;

        public Rigidbody Rigidbody;
        public float LookaheadOrientationStrength = 1f;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Collision")]*/
        [Tooltip("What is collided with to move the Camera closer to the Player")]
        public LayerMask CollisionLayers;
        public float CollisionReactSpeed = 10f;


        private float _targetScale = 1f;
        private float _currentScale = 1f;
        private Vector3 _allowanceOffset = Vector3.zero;
        private Vector3 _horPos = Vector3.zero;
        private Vector3 _vertPos = Vector3.zero;
        private CameraPropertiesVolume _currentVolume;
        private Vector3 _shoulderPosition = Vector3.zero;
        private float _goalShoulderPosition = 0f;


        private void Update()
        {
            if (AutoSwitch)
            {
                if (Shoulder == Shoulders.Left)
                {
                    if (Physics.SphereCast(Root.position + Vector3.up, 0.25f, -Root.right, out RaycastHit shoulderHit, 1, CollisionLayers))
                    {
                        Shoulder = Shoulders.Right;
                    }
                }
                else
                {
                    if (Physics.SphereCast(Root.position + Vector3.up, 0.25f, Root.right, out RaycastHit shoulderHit, 1, CollisionLayers))
                    {
                        Shoulder = Shoulders.Left;
                    }
                }
            }

            _shoulderPosition.y = ShoulderOffset.y;
            _shoulderPosition.z = -1;
            _goalShoulderPosition = ShoulderOffset.x * (((int)Shoulder * 2f) - 1f);
            _shoulderPosition.x = Mathf.Lerp(_shoulderPosition.x, _goalShoulderPosition, ShoulderSwitchSpeed * Time.deltaTime);
            Camera.localPosition = _shoulderPosition;

            var prevRootPos = Root.position;
            transform.position = Root.position;
            Root.position = prevRootPos;

            _allowanceOffset.y = -YDistanceAllowance;

            _horPos.x = Mathf.Lerp(Root.position.x, Follow.position.x + FollowOffset.x, HorizontalFollowSpeed * Time.deltaTime);
            _horPos.z = Mathf.Lerp(Root.position.z, Follow.position.z + FollowOffset.z, HorizontalFollowSpeed * Time.deltaTime);

            _vertPos.y = Mathf.Lerp(Root.position.y, Follow.position.y + FollowOffset.y, VerticalFollowSpeed * Time.deltaTime);


            Root.position = _horPos + _vertPos;


            // Apply rotation to Root
            _goalRotation = Quaternion.Euler(_yAxis, _xAxis, 0);

            if(Application.isPlaying)


            // Rotation smoothing
            _actualYAxis = Mathf.Lerp(_actualYAxis, _yAxis, SmoothSpeed * Time.deltaTime);
            _actualXAxis = Mathf.Lerp(_actualXAxis, _xAxis, SmoothSpeed * Time.deltaTime);

            Root.rotation = Quaternion.Euler(_actualYAxis, _actualXAxis, 0);

            Vector3 origin = Root.position;
            Vector3 direction = -Root.forward;

            if (Physics.SphereCast(origin, CameraCollisionRadius, direction, out RaycastHit hit, MaxDistance, CollisionLayers, QueryTriggerInteraction.Ignore))
            {
                // Reduce distance based on hit
                float hitDistance = Mathf.Clamp(hit.distance, 0.5f, MaxDistance);
                _targetScale = hitDistance;
            }
            else
            {
                _targetScale = MaxDistance;
            }

            // Smooth scale interpolation
            _currentScale = Mathf.Lerp(_currentScale, _targetScale, CollisionReactSpeed * Time.deltaTime);

            Root.localScale = new Vector3(1, 1, _currentScale);

            UpdateAimDirection();

            if (Application.isPlaying)
            {
                GoalLookDirection?.Invoke(_goalRotation * Vector3.forward);
                ActualLookDirection?.Invoke(Root.forward);

                if (Physics.Raycast(Camera.position, Camera.forward, out RaycastHit aimHit, Mathf.Infinity))
                {
                    AimPosition?.Invoke(aimHit.point);
                }
            }
        }


        void SwitchShoulder()
        {
            if (Shoulder == Shoulders.Left)
                Shoulder = Shoulders.Right;
            else
                Shoulder = Shoulders.Left;
        }

        private void OnTriggerEnter(Collider other)
        {
            var propVolume = other.GetComponent<CameraPropertiesVolume>();
            if (propVolume != null)
            {
                _currentVolume = propVolume;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var propVolume = other.GetComponent<CameraPropertiesVolume>();
            if (propVolume == _currentVolume)
            {
                _currentVolume = null;
            }
        }
    }
}