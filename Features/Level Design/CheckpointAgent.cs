using Remedy.Framework;
using UnityEngine;

namespace Remedy.LevelDesign
{
    public class CheckpointAgent : MonoBehaviour
    {
        public SignalData Respawn;
        private Rigidbody _rb => gameObject.GetCachedComponent<Rigidbody>();

        private void OnEnable()
        {
            Respawn?.Subscribe(this, (Vector3 position) =>
            {
                if (_rb != null)
                {
                    bool originallyKinematic = _rb.isKinematic;
                    _rb.isKinematic = true;
                    _rb.position = position;
                    _rb.isKinematic = originallyKinematic;
                }
            });
        }
    }
}