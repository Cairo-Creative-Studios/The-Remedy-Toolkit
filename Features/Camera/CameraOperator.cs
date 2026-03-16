/*using SaintsField;
using SaintsField.Playa;*/
using Unity.VisualScripting;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace Remedy.Cameras
{

    public class CameraOperator : MonoBehaviour
    {
        public SignalData SetObjectToFollow;
        public SignalData LookInput;

        [Tooltip("A direction value is passed, which is the horizontal direction the Camera is facing.")]
        public SignalData AimDirectionOutput;
        [Tooltip("A Boolean that tells the Rig that the Character Should look toward the AimDirectionOutput value.")]
        public SignalData OrientCharacterToCameraOutput;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./References")]
        [Expandable]*/
        [SchematicProperties]
        public CameraOperatorProperties Properties;
        public Transform Follow;
        public Transform Root;
        public Transform Camera;

        public float XAxisSpeed => Properties.XAxisSpeed;
        public float YAxisSpeed => Properties.YAxisSpeed;
        public float SmoothSpeed => Properties.SmoothSpeed;
        [Tooltip("If True, the Camera Manager will follow refer to this Operator's Current Volume for Camera Properties.")]
        public bool UseVolumes => Properties.UseVolumes;

        [Tooltip("Radius of the Sphere used for SphereCasting to set the distance away from the player")]
        public float CameraCollisionRadius = 0.2f;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Variables")]*/
        public CameraPropertiesVolume CurrentVolume;
        public Vector3 AimDirection;
        public bool OrientCharacterToCamera = false;

        private float _xAxisInput;
        private float _yAxisInput;

        /*[ReadOnly] */public float _xAxis;
        /*[ReadOnly*/ public float _yAxis;
        /*[ReadOnly] */public float _actualXAxis;
        /*[ReadOnly] */public float _actualYAxis;
        protected float _timeSinceInput = 0f;
        protected Quaternion _goalRotation;

        private void OnEnable()
        {
            Camera.gameObject.GetComponent<Camera>().enabled = false;
            CameraManager.SetCameraOperator(this);

            LookInput?.Subscribe(this, (Vector2 val) => Look(val));
            SetObjectToFollow?.Subscribe(this, (GameObject val) => {
                Follow = val.transform;
            });
        }

        private void OnDisable()
        {
            LookInput?.Unsubscribe(this);
            SetObjectToFollow?.Unsubscribe(this);
        }

        private void Reset()
        {
            // Set up hierarchy
            Root = new GameObject("Root").transform;
            Root.parent = transform;
            Root.localPosition = Vector3.zero;

            Camera = new GameObject("Camera").transform;
            Camera.gameObject.AddComponent<Camera>();
            Camera.parent = Root;
            Camera.localPosition = new Vector3(0, 0, -1);

            var baseDirection = Root.position - Camera.position;
            Camera.rotation = Quaternion.LookRotation(baseDirection, Vector3.up);

            gameObject.AddComponent<Rigidbody>().isKinematic = true;
            gameObject.AddComponent<SphereCollider>();
        }

        public void UpdateAimDirection()
        {
            AimDirection = Vector3.ProjectOnPlane(Camera.forward, Vector3.up).normalized;
            AimDirectionOutput?.Invoke(AimDirection);

            _xAxis += _xAxisInput * (XAxisSpeed * Time.deltaTime);
            _yAxis = Mathf.Clamp(_yAxis - _yAxisInput * (YAxisSpeed * Time.deltaTime), -80f, 80f); // clamp pitch (Z axis rotation)

            _xAxisInput = 0;
            _yAxisInput = 0;
        }

        public void ForceAxis(float xAxis, float yAxis)
        {
            _xAxis = xAxis;
            _actualXAxis = xAxis;
            _yAxis = yAxis;
            _actualYAxis = yAxis;
        }

        public void Look(Vector2 axis)
        {
            _xAxisInput = axis.x;
            _yAxisInput = axis.y;

            if (axis != Vector2.zero)
                _timeSinceInput = 0;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Root == null || Camera == null)
                return;

            // Draw wire cylinder for boom arm
            Vector3 start = Root.position;
            Vector3 end = Camera.position;
            float radius = CameraCollisionRadius;

            Gizmos.color = Color.cyan;
            DrawWireCylinder(start, end, radius);

            // Draw wire sphere at camera
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Camera.position, 0.2f);
        }

        void DrawWireCylinder(Vector3 start, Vector3 end, float radius)
        {
            const int segments = 20;
            Vector3 up = (end - start).normalized;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized;

            float height = Vector3.Distance(start, end);
            Quaternion rotation = Quaternion.LookRotation(end - start);

            Vector3[] points = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = 2 * Mathf.PI * i / segments;
                Vector3 circlePoint = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                points[i] = start + rotation * circlePoint;
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                Gizmos.DrawLine(points[i], points[next]);
                Gizmos.DrawLine(points[i], points[i] + up * height);
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                Gizmos.DrawLine(points[i] + up * height, points[next] + up * height);
            }
        }
#endif
    }
}