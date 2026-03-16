using Remedy.Framework;
using UnityEngine;

namespace Remedy.CharacterControllers.Hover
{
    [RequireComponent(typeof(CharacterMotionContext))]
    [RequireComponent(typeof(CharacterRaycastContext))]
    [SchematicComponent("Movement/3D/Flight and Hover Controller")]
    public class HoverController : MonoBehaviour
    {
        [SchematicEventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.JumpInput))]
        public Signal<bool> JumpInput => MotionContext.JumpInput;
        [SchematicEventLink(typeof(CharacterMotionContext), nameof(CharacterMotionContext.MoveInput))]
        public Signal<Vector2> MoveInput => MotionContext.MoveInput;

        public Signal<float> StaminaUpdated => MotionContext.StaminaUpdated;

        [SchematicProperties]
        public HoverControllerProperties Properties;

        public float CurrentVerticalSpeed = 0f;
        public float RotationVelocity;
        public CharacterMotionContext MotionContext => _motionContext ??= gameObject.GetComponent<CharacterMotionContext>();

        // cache
        private bool _cached = false;
        private CharacterMotionContext _motionContext;

        // state variables
        private Vector3 _moveInput;
        private float _currentHoverTime = 0f;
        private float _currentFallSpeed = 0f;

        private void OnEnable()
        {
            _motionContext.IsKinematic = false;

            Cache();

            MoveInput?.Subscribe(this, (Vector2 val) => Move(val));
            JumpInput?.Subscribe(this, (bool val) => Jump(val));

            _currentHoverTime = 0f;
            _currentFallSpeed = 0f;
        }

        private void Cache()
        {
            if (!_cached)
            {
                _motionContext = gameObject.GetCachedComponent<CharacterMotionContext>();
                _cached = true;
            }
        }

        private void OnDisable()
        {
            MoveInput?.Unsubscribe(this);
            JumpInput?.Unsubscribe(this);
        }

        private void FixedUpdate()
        {
            if(_motionContext.CurrentStamina > Properties.staminaUseRate * Time.fixedDeltaTime)
            {
                _motionContext.ApplyVerticalForce(Properties.RiseSpeed);
            }
            else
            {
                if(_currentHoverTime < Properties.HoverTime)
                {
                    _motionContext.ApplyVerticalForce(-Properties.HoverFallSpeed, true);
                    _currentHoverTime += Time.fixedDeltaTime;
                    _currentFallSpeed = 0f;
                }
                else
                {
                    if(_currentFallSpeed < Properties.PlummetTerminalSpeed)
                        _currentFallSpeed += Properties.PlummetAcceleration;

                    _motionContext.ApplyVerticalForce(-_currentFallSpeed, true);
                }
            }

            _motionContext.ApplyCappedHorizontalForce(_moveInput, Properties.HorizontalSpeed, Properties.HorizontalAcceleration, Properties.HorizontalDeceration);
        }

        public void Jump(bool value)
        {
            _moveInput.y = value ? 1 : 0;
        }

        public void Move(Vector2 value)
        {
            if(value != default)
            {
                _moveInput.x = value.x;
                _moveInput.z = value.y;
            }
        }
    }
}