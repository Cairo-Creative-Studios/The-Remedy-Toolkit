using Remedy.Framework;
using Remedy.Schematics.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace Remedy.CharacterControllers.LedgeGrab
{
    [RequireComponent(typeof(CharacterMotionContext))]
    [RequireComponent(typeof(CharacterRaycastContext))]
    [SchematicComponent("Movement/3D/Wall Slide Controller")]
    public class LedgeGrabController : MonoBehaviour
    {
        public Rigidbody Rigidbody => gameObject.GetCachedComponent<Rigidbody>();

        [SchematicEventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.JumpInput))]
        public Signal<bool> JumpInput => _motionContext.JumpInput;
        [SchematicEventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.MoveInput))]
        public Signal<Vector2> MoveInput => _motionContext.MoveInput;

        public SignalData OnClamber;
        public SignalData OnLedgeGrabExitted;

        [SchematicProperties]
        public LedgeGrabControllerProperties Properties;

        public Quaternion FacingRotation;

        public enum WallSide { Left, Right }
        public WallSide CurrentWallSide { get; private set; }

        public RaycastHit _lastWallHit;
        private Vector3 _facingDir;

        private bool _cached;
        private CharacterMotionContext _motionContext;
        private CharacterRaycastContext _raycastContext;
        private Transform _transform;
        private List<Vector3> _clamberPoints = new();

        private void OnEnable()
        {
            _cached = false;
            Cache();

            JumpInput?.Subscribe(this, (bool val) => Jump(val)); 

            _motionContext.WallCheckInMoveDirection();
            if (_raycastContext.WallCastResult.TryGetClosestHit(out RaycastHit hit))
            {
                StickOnWall(hit);
            }
        }

        private void OnDisable()
        {
            JumpInput?.Unsubscribe(this);
        }

        public void Jump(bool value)
        {
            if (value)
            {
                _clamberPoints[0] = _transform.position + Properties.ClamberHeight * Vector3.up;
                _clamberPoints[1] = (_transform.position + Properties.ClamberHeight * Vector3.up) + -_lastWallHit.normal * Properties.ClamberForward;
                _motionContext.FollowSpringPath(_transform.position.y, _clamberPoints);
                OnClamber?.FinalInvoke(default);
            }
        }

        private void Cache()
        {
            if (!_cached)
            {
                _motionContext = GetComponent<CharacterMotionContext>();
                _raycastContext = GetComponent<CharacterRaycastContext>();
                _transform = transform;
                _clamberPoints.Capacity = 2;
                _cached = true;
            }
        }

        private void StickOnWall(RaycastHit hit)
        {
            _lastWallHit = hit;

            _facingDir = -_lastWallHit.normal;

            FacingRotation = Quaternion.LookRotation(_facingDir, Vector3.up);

            float side = Vector3.Dot(_facingDir, _motionContext.HorizontalDirection);
            CurrentWallSide = side > 0 ? WallSide.Left : WallSide.Right;

            var ledgeResult = RaycastUtility.SphereCast(_transform, _transform.position, _facingDir, Properties.LedgeCheckRadius, _raycastContext.Properties.CollisionMask, -1, _raycastContext.Properties.MaxGroundSlopeAngle);

            if (_lastWallHit.distance < Properties.GapFromWall)
            {
                if (ledgeResult.TryGetClosestHit(out RaycastHit ledgeHit))
                {
                    var ledgeProjectedPosition = Vector3.ProjectOnPlane(_transform.position, ledgeHit.normal);
                    var wallProjectedPosition = Vector3.ProjectOnPlane(_transform.position, _lastWallHit.normal);

                    ledgeProjectedPosition.y = 0;
                    wallProjectedPosition.x = 0;
                    wallProjectedPosition.y = 0;

                    var actualPosition = ledgeProjectedPosition + wallProjectedPosition;

                    _motionContext.TeleportToPosition(actualPosition + _lastWallHit.normal * Properties.GapFromWall);
                }
            }
        }
    }
}