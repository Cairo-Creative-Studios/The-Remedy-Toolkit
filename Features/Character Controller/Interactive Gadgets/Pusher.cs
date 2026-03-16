using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Remedy.CharacterControllers.Gadgets
{
    [RequireComponent(typeof(SphereCollider))]
    public class Pusher : MonoBehaviour
    {
        [Header("Output Events")]
        [Tooltip("Invoked when a Rigidbody Collides with this.")]
        public SignalData OnHit;

        [Header("Properties")]
        public float Force;
        public Vector3 ScaleOnHit = Vector3.one;
        public int Delay = 30;

        private Vector3 _defaultScale;
        private float _waitTime = 0;

        private void Reset()
        {
            GetComponent<SphereCollider>().isTrigger = true;
        }

        private void OnEnable()
        {
            _defaultScale = transform.localScale;
        }

        private void Update()
        {
            _waitTime += Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            var rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                ApplyForce(rb).Forget();
                transform.localScale = Vector3.Scale(_defaultScale, ScaleOnHit);

                OnHit?.FinalInvoke(default);
            }
        }

        private async UniTaskVoid ApplyForce(Rigidbody rb)
        {
            await UniTask.Delay(Delay);
            rb.linearVelocity = transform.up * Force;
            transform.localScale = _defaultScale;
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject == null ||
                UnityEditor.Selection.activeGameObject.GetComponent<Pusher>() == null) return;
            Gizmos.color = Color.cyan;

            Vector3 initialVelocity = transform.up * Force;

            Vector3[] trajectoryPoints = CharacterTrajectoryCalculator.CalculateTrajectory(
                transform.position,
                initialVelocity,
                timeStep: Time.fixedDeltaTime,
                maxTime: 3f,
                maxPoints: 200,
                true
            );

            for (int i = 0; i < trajectoryPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(trajectoryPoints[i], trajectoryPoints[i + 1]);
            }

            if (trajectoryPoints.Length > 1)
            {
                Vector3 end = trajectoryPoints[^1];
                Vector3 prev = trajectoryPoints[^2];
                Vector3 direction = (end - prev).normalized;

                float headSize = 0.5f;
                Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
                Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -150, 0) * Vector3.forward;

                Gizmos.DrawLine(end, end + right * headSize);
                Gizmos.DrawLine(end, end + left * headSize);
            }
#endif
        }
    }
}
