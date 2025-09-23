using Remedy.Framework;
using UnityEngine;

namespace Remedy.CharacterControllers.WallSlide
{
    [RequireComponent(typeof(CharacterMotionContext))]
    [RequireComponent(typeof(CharacterRaycastContext))]
    [SchematicComponent("Movement/3D/Wall Slide Controller")]
    public class WallSlideController : MonoBehaviour
    {
        public Rigidbody Rigidbody => gameObject.GetCachedComponent<Rigidbody>();

        [EventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.JumpInput))]
        public ScriptableEventBoolean.Input JumpInput => _motionContext.JumpInput;
        [EventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.MoveInput))]
        public ScriptableEventVector2.Input MoveInput => _motionContext.MoveInput;

        public ScriptableEvent.Output OnWallJump = new();
        public ScriptableEvent.Output OnWallSlideExitted = new();

        [SchematicProperties]
        public WallSlideControllerProperties Properties;

        public Quaternion FacingRotation;

        public enum WallSide { Left, Right }
        public WallSide CurrentWallSide { get; private set; }

        public RaycastHit _lastWallHit;
        private Vector3 _facingDir;

        private bool _cached;
        private CharacterMotionContext _motionContext;
        private CharacterRaycastContext _raycastContext;
        private Transform _transform;

        private void OnEnable()
        {
            _cached = false;
            Cache();

            JumpInput?.Subscribe(this, Jump);
        }

        private void OnDisable()
        {
            JumpInput?.Unsubscribe(this);
        }

        private void FixedUpdate()
        {
            _motionContext.ApplyVerticalForce(-Properties.WallSlideSpeed, true);

            _motionContext.WallCheckInMoveDirection();
            if(_raycastContext.WallCastResult.TryGetClosestHit(out RaycastHit hit))
            {
                StickOnWall(hit);
            }
            else
            {
                OnWallSlideExitted?.Invoke(false);
            }
        }

        public void Jump(bool value)
        {
            if(value)
            {
                var lateralVelocity = Vector3.ProjectOnPlane(_motionContext.MoveDirection, _lastWallHit.normal);
                Vector3 jumpDir = (lateralVelocity * Properties.JumpLateralInputInfluence + _lastWallHit.normal).normalized;
                _motionContext.ApplyForce(jumpDir * Properties.JumpHorizontalForce + Vector3.up * Properties.JumpVerticalForce, true);
                
                OnWallJump?.Invoke(default);
            }
        }

        private void Cache()
        {
            if (!_cached)
            {
                _motionContext = GetComponent<CharacterMotionContext>();
                _raycastContext = GetComponent<CharacterRaycastContext>();
                _transform = transform;

                _cached = true;
            }
        }

        private void StickOnWall(RaycastHit hit)
        {
            _lastWallHit = hit;

            if (_lastWallHit.distance < Properties.GapFromWall)
            {
                var projectedTransform = Vector3.ProjectOnPlane(_transform.position, _lastWallHit.normal);
                _motionContext.TeleportToPosition(projectedTransform + _lastWallHit.normal * Properties.GapFromWall);
            }

            _facingDir = Vector3.ProjectOnPlane(_motionContext.HorizontalDirection, _lastWallHit.normal);

            if (_facingDir.sqrMagnitude < 0.001f)
                _facingDir = Vector3.ProjectOnPlane(-transform.right, _lastWallHit.normal);

            _facingDir.Normalize();

            FacingRotation = Quaternion.LookRotation(_facingDir, Vector3.up);

            float side = Vector3.Dot(_facingDir, _motionContext.HorizontalDirection);
            CurrentWallSide = side > 0 ? WallSide.Left : WallSide.Right;
        }
    }
}