/*using SaintsField;
using SaintsField.Playa;*/
using System;
using UnityEngine;

namespace Remedy.Weapons
{
    [SchematicComponent("Weapons/Holder")]
    /// <summary>
    /// The WeaponHolder component should be attached to a GameObject which is the child of a Character, Vehicle, or whatever, seperate from motion behaviour.
    /// It has a unique Collider (Trigger) that is used for interactions with Weapons that can be picked up
    /// </summary>
    //[Searchable]
    [RequireComponent(typeof(SphereCollider))]
    public class WeaponHolder : MonoBehaviour
    {
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Input")]*/
        [Tooltip("Device inputs that are passed to Equipped Weapons")]
        public WeaponInputEvent[] WeaponInputs;
        public SignalData EquipInput;
        [Tooltip("The current Aim Forward, updated from the Camera Controller.")]
        public SignalData AimInput;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./References")]*/
        public AimBone[] AimBones;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Sockets")]
        [InfoBox("The way a Weapon fills sockets is based on the number of (and SocketIndex of) WeaponHandles that are applied to the Weapon Prefab. " +
                "If there are two WeaponHandles, the weapon will need the two specified WeaponSockets open the WeaponHolder for it to be equipped.")]*/
        [Tooltip("Weapon Sockets are parent Transforms for Weapons that are added to this Weapon Holder and determine the orientation of them")]
        public WeaponSocket[] WeaponSockets;
        private WeaponSocket[] _previousWeaponSockets;

/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Variables")]*/
        [Tooltip("The weapon we're currently using.")]
        public int ActiveWeaponSlot = 0;


        private SphereCollider _collider;
        public SphereCollider Collider => _collider ??= GetComponent<SphereCollider>();

        private Vector3 _aimInput = Vector3.zero;

        void OnValidate()
        {
            WeaponSocket previousSocket = null;
            for (int i = 0; i < WeaponSockets.Length; i++)
            {
                var slot = WeaponSockets[i];
                if (slot == previousSocket && slot != null)
                {
                    WeaponSockets[i] = new(new GameObject("WeaponSocket_" + i).transform);
                }
            }
            _previousWeaponSockets = WeaponSockets;
        }

        void OnEnable()
        {
            EquipInput?.Subscribe(this, (int weaponID) => Equip(weaponID));

            foreach (var weaponInput in WeaponInputs)
            {
                weaponInput.InputEvent?.Subscribe(this, (bool value) =>
                {
                    if (weaponInput.Event == WeaponInputEvent.WeaponEventType.Fire)
                    {
                        Fire(weaponInput.Socket, weaponInput.Mode);
                    }
                    else
                    {
                        Reload(weaponInput.Socket, weaponInput.Mode);
                    }
                });
            }

            AimInput?.Subscribe(this, (Vector3 val) =>
            {
                foreach (var socket in WeaponSockets)
                {
                    if (socket == null || socket.ActiveWeapon == null) continue;
                    _aimInput = val;
                }
            });

            Collider.isTrigger = true;
            Collider.includeLayers = GlobalWeaponData.Instance.WeaponCollisionLayer;
        }

        private void Update()
        {
            foreach (var socket in WeaponSockets)
            {
                var HeldWeapon = socket.ActiveWeapon;
                if (HeldWeapon != null)
                {
                    if (HeldWeapon.PrimaryHandle != null)
                    {
                        HeldWeapon.PrimaryHandle.Transform.position = WeaponSockets[0].Transform.position;
                    }
                    HeldWeapon.AttackDirection = _aimInput;
                }
            }
        }

        /// <summary>
        /// Equips the Weapon with the given WeaponID (the ID within the WeaponData.Lookup)
        /// </summary>
        /// <param name="weaponID"></param>
        /// <param name="socket">The socket to place the Weapon in. If a weapon is in said socket, it will be replaced.</param>
        public void Equip(int weaponID, int socket = 0)
        {
            var weaponData = WeaponData.Lookup[weaponID];
            if (weaponData.Prefab != null)
            {
                var weapon = Instantiate(weaponData.Prefab);
                weapon.Equip(this);
                WeaponSockets[socket].ActiveWeapon = weapon;
            }
        }

        public void UnEquip(int socket = 0)
        {
            Destroy(WeaponSockets[socket].ActiveWeapon);
        }

        /// <summary>
        /// Shoot the given Weapon using the given Mode.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="mode"></param>
        //[Button]
        public void Fire(int socket = 0, int mode = 0)
        {
            WeaponSockets[socket]?.ActiveWeapon?.Fire(mode, true);
        }

        /// <summary>
        /// Reload the given Weapon for the given Mode.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="mode"></param>
        //[Button]
        public void Reload(int socket = 0, int mode = 0)
        {
            WeaponSockets[socket]?.ActiveWeapon?.Reload(mode).GetAsyncEnumerator();
        }
    }

    /// <summary>
    /// The Definition of a Weapon's Input Event. Use this to create special Inputs to pass to Equippable Weapons,
    /// allowing them to perform special functionality.
    /// </summary>
    [Serializable]
    public class WeaponInputEvent
    {
        [Tooltip("The Socket for which this functionality should occur.")]
        public int Socket;
        [Tooltip("The Mode for the Weapon that this is performed on, if the Weapon has different modes of Fire.")]
        public int Mode;
        public enum WeaponEventType
        {
            Fire,
            Reload
        }
        [Tooltip("The Unique Functionality that the Weapon should perform")]
        public WeaponEventType Event;

        [Tooltip("The ScriptableEvent that was triggered to invoke this Weapon Evnet")]
        public SignalData InputEvent;
    }

    /// <summary>
    /// Weapon Sockets are used for translating and orienting Weapons relative to the Weapon Holder.
    /// For a character, Socket Transforms would likely be parented to hand bones, so the character visually holds the Weapon.
    /// Orientation of the Weapon is handled per-Weapon in the Weapon Script.
    /// </summary>
    [Serializable]
    public class WeaponSocket
    {
        [Tooltip("The Name of the Slot, which can be used to Query the Transform of the WeaponSlot within the WeaponHolder.")]
        public string Name = "";
        [Tooltip("The object's existence in the world.")]
        public Transform Transform;
        [Tooltip("Optional ID for the Slot (for special cases of weapon Transformation).")]
        public int SpecialID = -1;
        [Tooltip("The Weapon in this Socket.")]
        public Weapon ActiveWeapon;
        public Vector3 PositionOffset;
        [Tooltip("The Offset at for the held Weapon rotationally from the bone socket's rotation.")]
        public Vector3 AngularOffset;

        public WeaponSocket(Transform transform)
        {
            Transform = transform;
        }
    }

    public class AimBone
    {
        public Transform Limb;
        public Vector3 EulerOffset = Vector3.zero;
        public float RedirectSpeed = 5f;
    }
}