using Remedy.Framework;
using Remedy.CharacterControllers.Hover;
using Remedy.CharacterControllers.LedgeGrab;
using Remedy.CharacterControllers.WallSlide;
/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.CharacterControllers
{

    //[Searchable]
    public class CharacterControllerRig : MonoBehaviour
    {
        public LayerMask CollisionLayer;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Input")]*/
        [Tooltip("Vector3 Input for the Aim Direction, should be passed from the Camera Manager.")]
        public SignalData InputAim;
        [Tooltip("Boolean Inputs that determine whether to Orient toward the Aim Direction.")]
        public SignalData InputOrientToAim;
        public SignalData OnGroundCast;
        public SignalData OnGroundCastHit;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        [Header("Events")]
        public SignalData IsWallSliding;
        public SignalData IsHovering;
        public SignalData IsLedgeGrabbing;
        public SignalData IsStrafing;
        public SignalData VerticalVelocity;
        public SignalData IsOnGround;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./References")]*/
        [Header("References")]
        public Transform CameraTransform;
        public Transform CharacterRenderer;
        [Tooltip("Bone that should be placed at the Ledge Position when Ledge Grabbing.")]
        public Transform LedgeGrabHandTransform;
        public Material ShadowDecal;
        public Transform ShadowProjector;

        public bool HasDecal => ShadowDecal != null;

/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Scale")]*/
        [Header("Shadow Decal")]
        public float DecalMinScale = 1.15f;
        public float DecalMaxScale = 1.35f;
        public float DecalScaleGap = 5f;

/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Motion")]*/
        public bool RotateCharacterWhileJumping = false;
        public Vector3 Offset = Vector3.zero;
        public float TiltDampening = 5f;
        public float TurnDamening = 15f;
        public float PositionFollowSpeed = 20f;
        public float ScaleSpringDampening = 10f;
        public float ScaleSpringFrequency = 6f;
        public float VerticalVelocityScaleFactor = 0.2f;
        public float OrientToCameraSpeed = 10f;

        public PhysicsBasedCharacterController _characterController;
        public PhysicsBasedCharacterController CharacterController => _characterController ??= SystemManager.GetCachedComponent<PhysicsBasedCharacterController>(gameObject);

        private WallSlideController _wallSlideController;
        public WallSlideController WallSlideController => _wallSlideController ??= gameObject.GetCachedComponent<WallSlideController>();

        private HoverController _hoverController;
        public HoverController HoverController => _hoverController ??= gameObject.GetCachedComponent<HoverController>();

        private LedgeGrabController _ledgeGrabController;
        public LedgeGrabController LedgeGrabController => _ledgeGrabController ??= gameObject.GetCachedComponent<LedgeGrabController>();

        public Rigidbody rb => CharacterController.GetCachedComponent<Rigidbody>();



/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Variables")]*/
        [SerializeField/*, ReadOnly*/] private Vector3 position;
        private Vector3 _localVelocity;
        private Vector3 Tilt = Vector3.zero;
        private Vector3 _scale;
        private Vector3 _handLocalOffset;
        private Vector3 _targetRendererPosition;
        private Vector3 _direction;
        private Vector3 _baseScale = Vector3.one;
        private float _goalYScale = 0;
        private Vector3 _aimDirection = Vector3.zero;
        private bool _orientToDirection = false;
        private bool _isGrounded = false;
        private RaycastHit _groundHit = default;
        private Transform _transform;

        float yVelocity = 0f;

        private void OnEnable()
        {
            InputAim?.Subscribe(this, (Vector3 val) =>
            {
                _aimDirection = val;
            });
            InputOrientToAim?.Subscribe(this, (bool val) =>
            {
                _orientToDirection = val;
            });
            OnGroundCast?.Subscribe(this, (bool val) =>
            {
                _isGrounded = val;
                IsOnGround?.Invoke(val);
            });
            OnGroundCastHit?.Subscribe(this, (RaycastHit hit) =>
            {
                _groundHit = hit;
            });

            _transform = transform;
        }

        private void OnDisable()
        {
            InputAim.Unsubscribe(this);
            InputOrientToAim.Unsubscribe(this);
            OnGroundCast.Unsubscribe(this);
            OnGroundCastHit.Unsubscribe(this);
        }

        private void Update()
        {
            if (ShadowDecal != null)
            {
                if (Physics.Raycast(_transform.position, Vector3.down, out RaycastHit hitInfo, 1000, CollisionLayer))
                {
                    float gap = Mathf.Clamp(hitInfo.distance / DecalScaleGap, 0, DecalScaleGap);
                    ShadowDecal.SetFloat("_Scale", Mathf.Lerp(DecalMaxScale, DecalMinScale, gap));
                }
            }

            _goalYScale = 1 + Mathf.Abs(rb.linearVelocity.y) * VerticalVelocityScaleFactor;

            float delta = _goalYScale - _baseScale.y;
            yVelocity += ScaleSpringFrequency * ScaleSpringFrequency * delta * Time.deltaTime;
            yVelocity *= Mathf.Exp(-ScaleSpringDampening * Time.deltaTime);
            _baseScale.y += yVelocity * Time.deltaTime;

            _direction = CharacterController.transform.position - CharacterRenderer.position;
            _direction.y = 0;

            VerticalVelocity?.Invoke(rb.linearVelocity.y);

            // Handle Wall Slide
            if (WallSlideController.enabled)
            {
                position.x = Mathf.Lerp(position.x, _transform.position.x, PositionFollowSpeed * Time.deltaTime);
                position.z = Mathf.Lerp(position.z, _transform.position.z, PositionFollowSpeed * Time.deltaTime);
                position.y = Mathf.Lerp(position.y, _transform.position.y - Offset.y, PositionFollowSpeed * Time.deltaTime);

                CharacterRenderer.position = position;

                CharacterRenderer.rotation = WallSlideController.FacingRotation;

                _scale.x = WallSlideController.CurrentWallSide == WallSlideController.WallSide.Left ? 1 : -1;
                _scale.y = _baseScale.y;
                _scale.z = _baseScale.z;

                CharacterRenderer.localScale = _scale;

                IsWallSliding?.Invoke(true);
            }
            else if (HoverController.enabled)
            {
                _localVelocity = CharacterRenderer.InverseTransformDirection(rb.linearVelocity);

                Tilt.x = _localVelocity.z * 2.5f; // Forward/backward tilt (pitch)
                Tilt.z = -_localVelocity.x * 2.5f;  // Side-to-side tilt (roll)

                if (!_orientToDirection)
                    CharacterRenderer.rotation = Quaternion.Lerp(CharacterRenderer.rotation, Quaternion.LookRotation(_direction, CharacterRenderer.up), TurnDamening * Time.deltaTime);
                else
                    CharacterRenderer.rotation = Quaternion.Slerp(CharacterRenderer.rotation, Quaternion.LookRotation(_aimDirection, Vector3.up), OrientToCameraSpeed * Time.deltaTime);

                Tilt.y = CharacterRenderer.eulerAngles.y;

                CharacterRenderer.eulerAngles = Tilt;

                position.x = Mathf.Lerp(position.x, _transform.position.x, PositionFollowSpeed * Time.deltaTime);
                position.z = Mathf.Lerp(position.z, _transform.position.z, PositionFollowSpeed * Time.deltaTime);
                position.y = Mathf.Lerp(position.y, _transform.position.y, 5 * Time.deltaTime);

                CharacterRenderer.position = position;

                IsHovering?.Invoke(true);
                IsWallSliding?.Invoke(false);


                _scale.x = _baseScale.x;
                _scale.y = _baseScale.y;
                _scale.z = _baseScale.z;

                CharacterRenderer.localScale = _scale;
            }

            else if (LedgeGrabController.enabled)
            {
                // Calculate local space offset between hand and renderer
                _handLocalOffset = CharacterRenderer.InverseTransformPoint(LedgeGrabHandTransform.position);

                // Target position is LedgePosition - that offset in world space
                //_targetRendererPosition = LedgeGrabController.LedgePosition - CharacterRenderer.TransformVector(_handLocalOffset);

                // Smoothly interpolate renderer position to target
                if (Vector3.Distance(_transform.position, _targetRendererPosition) < 5f)
                    position = Vector3.Lerp(CharacterRenderer.position, _targetRendererPosition, PositionFollowSpeed * Time.deltaTime);
                CharacterRenderer.position = position;
/*
                if (LedgeGrabController.CurrentWallSide == LedgeGrabController.WallSide.Left)
                    CharacterRenderer.rotation = LedgeGrabController.FacingRotation;
                if (LedgeGrabController.CurrentWallSide == LedgeGrabController.WallSide.Right)
                    CharacterRenderer.rotation = Quaternion.Euler(LedgeGrabController.FacingRotation.eulerAngles + new Vector3(0, 180, 0));*/

                _scale.x = _baseScale.x;
                _scale.y = _baseScale.y;
                _scale.z = _baseScale.z;

                CharacterRenderer.localScale = _scale;

                IsLedgeGrabbing?.Invoke(true);
                IsWallSliding?.Invoke(false);
                IsHovering?.Invoke(false);
            }
            else
            {

                IsLedgeGrabbing?.Invoke(false);
                IsWallSliding?.Invoke(false);
                IsHovering?.Invoke(false);

                CharacterRenderer.localScale = Vector3.one;

                if (_groundHit.point != Vector3.zero)
                {
                    var distanceFromFloor = _groundHit.distance;

                    position.y = Mathf.Lerp(position.y, distanceFromFloor, PositionFollowSpeed * Time.deltaTime);
                }
                else
                {
                    position.y = Mathf.Lerp(position.y, _transform.position.y - Offset.y, PositionFollowSpeed * Time.deltaTime);
                }

                position.x = Mathf.Lerp(position.x, _transform.position.x, PositionFollowSpeed * Time.deltaTime);
                position.z = Mathf.Lerp(position.z, _transform.position.z, PositionFollowSpeed * Time.deltaTime);

                CharacterRenderer.position = position;



                if (Mathf.Abs(rb.linearVelocity.x) + Mathf.Abs(rb.linearVelocity.z) > 0.5f)
                {
                    Tilt.x = Mathf.LerpAngle(CharacterRenderer.eulerAngles.x, CharacterController.transform.eulerAngles.x, TiltDampening * Time.deltaTime);
                    Tilt.z = Mathf.LerpAngle(CharacterRenderer.eulerAngles.z, CharacterController.transform.eulerAngles.z, TiltDampening * Time.deltaTime);

                    if (!_orientToDirection)
                        if (RotateCharacterWhileJumping || _isGrounded)
                            CharacterRenderer.rotation = Quaternion.Lerp(CharacterRenderer.rotation, Quaternion.LookRotation(_direction, CharacterRenderer.up), TurnDamening * Time.deltaTime);

                    if (_orientToDirection)
                        CharacterRenderer.rotation = Quaternion.Slerp(CharacterRenderer.rotation, Quaternion.LookRotation(_aimDirection, Vector3.up), OrientToCameraSpeed * Time.deltaTime);
                }
                else
                {
                    Tilt.x = Mathf.LerpAngle(CharacterRenderer.eulerAngles.x, 0, TurnDamening * Time.deltaTime);
                    Tilt.z = Mathf.LerpAngle(CharacterRenderer.eulerAngles.z, 0, TurnDamening * Time.deltaTime);

                    if (_orientToDirection)
                        CharacterRenderer.rotation = Quaternion.Slerp(CharacterRenderer.rotation, Quaternion.LookRotation(_aimDirection, Vector3.up), OrientToCameraSpeed * Time.deltaTime);

                }

                Tilt.y = CharacterRenderer.eulerAngles.y;
                CharacterRenderer.eulerAngles = Tilt;

                _scale.x = _baseScale.x;
                _scale.y = _baseScale.y;
                _scale.z = _baseScale.z;

                CharacterRenderer.localScale = _scale;
            }

            ShadowProjector.rotation = Quaternion.LookRotation(Vector3.down, Vector3.up);
        }
    }
}