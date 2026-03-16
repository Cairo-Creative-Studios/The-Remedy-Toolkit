/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.Weapons.Projectiles
{
    [SchematicComponent("Weapons/Projectile")]
    //[Searchable]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class Projectile : MonoBehaviour
    {
/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        [Tooltip("On hit anything collidable, passes the PhysicMaterial of the collider that was hit.")]
        public SignalData OnHit;
        [Tooltip("On hit the Target Object, passes the reference of the Target Object that was hit.")]
        public SignalData OnHitTarget;
        [Tooltip("On Hit of anything collidable, and the projectile bounced off, passes the PhysicMaterial of the hit Collider")]
        public SignalData OnRicochet;
        [Tooltip("Called when this is destroyed")]
        public SignalData OnDestroyed;

/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]
        [Expandable]*/
        public ProjectileData Data;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Variables")]*/
        [Tooltip("Used for magnetization or orientation to the Target object.")]
        public Transform Target;
        [Tooltip("The position to interpolate toward until it has been reached.")]
        public Vector3 TargetPosition; 
        [Tooltip("Speed of motion")]
        public Vector3 Velocity;
        // This should probably be deleted, Collisions should be handled by collision layer only 
        [Tooltip("The current number of collisions")]
        public int CollisionCount;
        
        private Rigidbody _rigidBody;
        public Rigidbody RigidBody => _rigidBody ??= GetComponent<Rigidbody>();

        private SphereCollider _collider;
        public SphereCollider Collider => _collider ??= GetComponent<SphereCollider>();

        public FireMode ParentFireMode;

        public Vector3 AimTargetPosition = Vector3.zero;

        private float _timeAlive = 0f;
        private float _distanceTraveled = 0f;

        void Start()
        {
            Velocity = Data.InitialVelocity * transform.forward;
            RigidBody.useGravity = false;
            gameObject.layer = Data.Layer;
            RigidBody.isKinematic = true;
            Collider.isTrigger = true;
        }

        private void Update()
        {
            _distanceTraveled += (Velocity * Time.deltaTime).magnitude;

            if(_distanceTraveled < Data.LineUpDistance)
            {
                Vector3 offsetPosition = Vector3.Lerp(transform.position, AimTargetPosition, _distanceTraveled / Data.LineUpDistance);
                transform.position = offsetPosition;

                if (Vector3.Distance(transform.position, AimTargetPosition) < 1)
                    _distanceTraveled = Data.LineUpDistance;
            }

            if (Physics.SphereCast(transform.position, Collider.radius, Velocity.normalized, out RaycastHit hit, Velocity.magnitude * Time.deltaTime))
            {
                OnTriggerEnter(hit.collider);
            }

            transform.position += Velocity * Time.deltaTime;

            _timeAlive += Time.deltaTime;

            if (_timeAlive > Data.LifeSpan)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {

            if(Data.TargetingMode == ProjectileData.TargetMode.MatchingTag)
            {
                if (other.gameObject.tag == Data.TargetTag)
                {
                    OnHitTarget?.Invoke(other.gameObject);
                    OnHit?.Invoke(other.sharedMaterial);
                }
            }
            else
            {
                if (other.gameObject == Target.gameObject)
                {
                    OnHitTarget?.Invoke(other.gameObject);
                    OnHit?.Invoke(other.sharedMaterial);
                }
            }

            OnHit?.Invoke(other.sharedMaterial);
        }

        // Draws a Gizmo displaying the path to the next frame's position
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return; 

            Gizmos.color = Color.red;

            Vector3 nextPosition = transform.position + Velocity * Time.deltaTime;
            float castDistance = Velocity.magnitude * Time.deltaTime;

            Gizmos.DrawWireSphere(transform.position, Collider ? Collider.radius : 0.1f);

            Gizmos.DrawLine(transform.position, nextPosition);

            if (Physics.SphereCast(transform.position, Collider.radius, Velocity.normalized, out RaycastHit hit, castDistance))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(hit.point, Collider.radius);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(nextPosition, Collider.radius);
            }
        }

    }
}