//using SaintsField;
using UnityEngine;
using System;
//using SaintsField.Playa;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Remedy.Framework;
using Remedy.Weapons.Projectiles;
using Remedy.Common;
using Remedy.Schematics.Utils;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Remedy.Weapons
{
    [SchematicComponent("Weapons/Weapon")]
    //[Searchable]
    public class Weapon : MonoBehaviour
    {
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        [Tooltip("Invoked when the Weapon is picked up by a WeaponHolder. The GameObject possessing the WeaponHolder Component is passed.")]
        public SignalData OnPickedup;
        [Tooltip("Invoked when the Weapon is dropped by a WeaponHolder")]
        public SignalData OnDropped;

        [Tooltip("Invoked after this Fire Mode shoots, intended to pull the Object containing the Weapon toward the direction it is Attacking.")]
        public SignalData OnPull;
        [Tooltip("Invoked before this Fire Mode shoots, redirecting the object toward the best Target, rotationally (must be handled externally)")]
        public SignalData OnRedirectDirection;
        [Tooltip("Invoked before this Fire Mode shoots, redirecting the object toward the best Target, positionally (must be handled externally)")]
        public SignalData OnRedirectPosition;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Pieces")]
        [InfoBox("When two Handles are added to the Weapon, the Weapon will use the first for translation, and the angle to the next one for rotation.")]*/
        [Tooltip("The handles of the Weapon, used to place the Weapon into a Weapon Holder's Weapon Socket.")]
        public WeaponHandle PrimaryHandle;
        public WeaponHandle SecondHandle;
/*
        [InfoBox("Muzzles are paired with their FireModes here. (recieved from Weapon Data, in Component/Properties). This lets you add" +
            "different modes of fire, hence the name. For example, a Grenade Launcher attachment on an assault rifle, or the different " +
            "modes of fire available for each Unreal Tournament weapon.")]
        [InfoBox("When a WeaponHolder fires or reloads, it finds the correct FireMode to act upon by it's Index in this list.")]*/
        [Tooltip("The Transforms of the Muzzles to use for the FireModes of the Weapons.")]
        [IdentityListRenderer(identifierType: ListIdentifierType.Name, identifierField: "Name", depth: 0)]
        public FireModeMuzzlePair[] FireModeMuzzles;

/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]*/
        [Tooltip("If true, the weapon will be placed in the world.")]
        public bool InWorld = true;
        [Tooltip("If the Weapon is not being held, and this is true, it will use Rigidbody physics.")]
        public bool UsePhysics = true;
        [Tooltip("The Data of the Weapon, as a Scriptable Object")]
        //[Expandable]
        public WeaponData Data;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Variables")]*/
        [Tooltip("The Holder Component of the object holding, controlling and managing this Weapon.")]
        public WeaponHolder Holder;
        [Tooltip("The Rigidbody of the WeaponHolder")]
        public Rigidbody WeaponHolderRigidbody;
        public Vector3 AttackDirection;

        // TODO: Replace with a system that logs prefab ID's in Weapon Data to make Weapon Systems more clear.
        public int ID;

        private Quaternion _aimTarget;
        private float LastFireTime;

        private void Awake()
        {
            WeaponHolderRigidbody = GetComponent<Rigidbody>();
            if (PrimaryHandle.Transform == null)
            {
                enabled = false;
                Debug.LogError("The Primary Handle of the Weapon hasn't been set, so it can't be used. Please set the Primary Handle Reference in the Inspector.", this);
            }
            else
                PrimaryHandle.Offset = transform.position - PrimaryHandle.Transform.position;


            for (int i = 0; i < FireModeMuzzles.Length; i++)
            {
                FireModeMuzzles[i].Data = Data;
            }
        }

        void OnValidate()
        {
            for (int i = 0; i < FireModeMuzzles.Length; i++)
            {
                FireModeMuzzles[i].Data = Data;
                // TODO: Instead of automating this process, add a Button for adding new Muzzles for a FireMode
                // draw some sphere gizmos of different colors to show the handles vs the muzzles when the weapon is selected.
                // drawing each FireMode's muzzles with different colors would be cool. Maybe draw handles as cubes and FireMode
                // muzzles as different color spheres. 
                /*            for (int j = 0; j < FireModeMuzzles[i].Muzzles.Count; j++)
                            {
                                if(FireModeMuzzles[i].Muzzles[j] == null)
                                {
                                    FireModeMuzzles[i].Muzzles[j] = new GameObject("Muzzle_" + i).transform;
                                    FireModeMuzzles[i].Muzzles[j].parent = transform;
                                }
                            }*/
            }
        }

        void Update()
        {
            if (PrimaryHandle.Socket == null || PrimaryHandle.Socket.Transform == null) return;

            Vector3 offset = PrimaryHandle.Socket.Transform.rotation * PrimaryHandle.Socket.PositionOffset;
            transform.position = PrimaryHandle.Socket.Transform.position + offset;

            // Rotation: if there's a second handle, aim at it
            if (SecondHandle.Transform != null)
            {
                Vector3 direction = (SecondHandle.Socket.Transform.position - PrimaryHandle.Socket.Transform.position).normalized;
                if (direction.sqrMagnitude > 0.0001f) // prevent NaN
                    transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                // Otherwise match the rotation of the primary handle
                transform.rotation = PrimaryHandle.Socket.Transform.rotation;
                transform.eulerAngles += PrimaryHandle.Socket.AngularOffset;
            }
        }

        /// <summary>
        /// Sets the Socket's for the Weapon's Handles to the Sockets of the WeaponHolder.
        /// </summary>
        /// <param name="holder"></param>
        public void Equip(WeaponHolder holder)
        {
            Holder = holder;

            for (int i = 0; i < holder.WeaponSockets.Length; i++)
            {
                if (holder.WeaponSockets[i].ActiveWeapon == null)
                {
                    if (SecondHandle.Transform != null)
                    {
                        if (holder.WeaponSockets.Length < i + 1 || holder.WeaponSockets[i] != null)
                            return;

                        PrimaryHandle.Socket = holder.WeaponSockets[i];
                        SecondHandle.Socket = holder.WeaponSockets[i + 1];
                    }
                    else
                    {
                        PrimaryHandle.Socket = holder.WeaponSockets[i];
                    }
                    break;
                }
            }
            OnPickedup?.Invoke(holder.gameObject);
        }

        public void Aim(Vector2 input, Transform _muzzleTransform)
        {
            foreach (var fireMode in FireModeMuzzles)
            {
                if (_aimTarget == null) _aimTarget = _muzzleTransform.rotation;

                if (_muzzleTransform == null)
                    return;

                switch (fireMode.FireMode.AimMode)
                {
                    case FireMode.AimMethod.AimDelta:
                        _aimTarget *= Quaternion.Euler(-input.y, input.x, 0);
                        break;

                    case FireMode.AimMethod.AimDirectly:
                        if (input.sqrMagnitude > 0.0001f)
                        {
                            float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
                            _aimTarget = Quaternion.Euler(0, angle, 0);
                        }
                        break;
                }
            }
        }

        public void Fire(int mode = 0, bool pressed = false)
        {
            var muzzleDefinition = FireModeMuzzles[mode];

            if ((muzzleDefinition.AmmoCount > 0 || muzzleDefinition.FireMode.InfiniteAmmo)
                    && !muzzleDefinition.IsReloading
                    && (Time.time >= LastFireTime + muzzleDefinition.FireMode.FireRate)
                    && (muzzleDefinition.FireMode.RapidFire || pressed))
            {
                List<Transform> muzzleTransforms = new();

                switch (muzzleDefinition.MuzzleSelection)
                {
                    case FireModeMuzzlePair.MuzzleSelectionStyle.All:
                        muzzleTransforms = muzzleDefinition.Muzzles;
                        break;
                    case FireModeMuzzlePair.MuzzleSelectionStyle.Random:
                        muzzleTransforms.Add(muzzleDefinition.Muzzles[UnityEngine.Random.Range(0, muzzleDefinition.Muzzles.Count)]);
                        break;
                    case FireModeMuzzlePair.MuzzleSelectionStyle.Incremental:
                        muzzleDefinition.MuzzleIndex = (muzzleDefinition.MuzzleIndex + 1) % muzzleDefinition.Muzzles.Count;
                        muzzleTransforms.Add(muzzleDefinition.Muzzles[muzzleDefinition.MuzzleIndex]);
                        break;
                }

                foreach (var muzzleTransform in muzzleTransforms)
                {
                    Projectile projectileController;

                    HandleRedirection(muzzleDefinition.FireMode, muzzleTransform);

                    muzzleDefinition.AmmoCount--;
                    LastFireTime = Time.time;

                    OnPull?.Invoke(muzzleTransform.forward * muzzleDefinition.FireMode.Pull);

                    // For each shot in a fire, determines the orientation unqiuely
                    switch (muzzleDefinition.FireMode.DisperseMethod)
                    {
                        case FireMode.ShotDisperseMethod.None:
                            for (int i = 0; i < muzzleDefinition.FireMode.ShotProjectileCount; i++)
                            {
                                projectileController = InstantiateProjectile(muzzleDefinition.FireMode, muzzleDefinition.FireMode.Projectile, muzzleTransform);
                                OrientToMuzzle(muzzleDefinition.FireMode, projectileController, muzzleTransform);
                            }
                            break;
                        case FireMode.ShotDisperseMethod.Spread:
                            for (int i = 0; i < muzzleDefinition.FireMode.ShotProjectileCount; i++)
                            {
                                projectileController = InstantiateProjectile(muzzleDefinition.FireMode, muzzleDefinition.FireMode.Projectile, muzzleTransform);
                                OrientToMuzzleWithSpread(muzzleDefinition.FireMode, projectileController, muzzleTransform);
                                HandleAffix(muzzleDefinition.FireMode, projectileController);
                            }
                            break;
                        case FireMode.ShotDisperseMethod.Incremental:
                            for (int i = 0; i < muzzleDefinition.FireMode.ShotProjectileCount; i++)
                            {
                                projectileController = InstantiateProjectile(muzzleDefinition.FireMode, muzzleDefinition.FireMode.Projectile, muzzleTransform);
                                OrientIncrementally(muzzleDefinition.FireMode, projectileController, i, muzzleTransform);
                                HandleAffix(muzzleDefinition.FireMode, projectileController);
                            }
                            break;
                    }
                }
                return;
            }
        }

        /// <summary>
        /// Calls Redirection Events based on the Type of Redirection to apply for this FireMode
        /// </summary>
        private void HandleRedirection(FireMode mode, Transform muzzleTransform)
        {
            switch (mode.RedirectionMode)
            {
                case FireMode.Redirection.NearestToMuzzleForward:
                    if (Physics.SphereCast(muzzleTransform.position, mode.RedirectionCheckRadius, muzzleTransform.forward, out RaycastHit hit, mode.MaxCheckDistance, mode.Projectile.Data.CollisionLayer))
                    {
                        Vector3 direction = hit.point - muzzleTransform.position;
                        if (direction.magnitude < 0.01f) break;

                        OnRedirectPosition?.Invoke(direction.normalized);
                    }
                    break;
            }
        }

        /// <summary>
        /// Create and set up the Projectile Instance and it's Controller
        /// </summary>
        /// <param name="projectile"></param>
        /// <returns></returns>
        private Projectile InstantiateProjectile(FireMode mode, Projectile projectile, Transform muzzleTransform)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = RemedyGameManager.MainCamera.ScreenPointToRay(screenCenter);

            var projectileInstance = Instantiate(projectile.gameObject, muzzleTransform.position, Quaternion.LookRotation(ray.direction));
            var projectileController = projectileInstance.GetComponent<Projectile>();
            projectileController.ParentFireMode = mode;

            // Camera cam = Camera.main;

            if (Physics.Raycast(ray, out RaycastHit hitInfo,
                                projectileController.Data.LineUpDistance,
                                projectileController.Data.CollisionLayer))
            {
                projectileController.AimTargetPosition = hitInfo.point;
            }
            else
            {
                projectileController.AimTargetPosition = ray.origin + ray.direction * projectileController.Data.LineUpDistance;
            }

            projectileController.OnDestroyed?.Subscribe(this, (Union value) =>
            {
                // TODO: Allow a ScriptableEvent or collection of events to be passed from the WeaponHolder to the Projectile,
                // So that functionality can be performed per-character per-weapon per-projectile
                //OnProjectileDestroyed?.Invoke(projectileController);
            });

            return projectileController;
        }
        /// <summary>
        /// Simply orients to the Rotation of the Muzzle
        /// </summary>
        /// <param name="projectile"></param>
        private void OrientToMuzzle(FireMode mode, Projectile projectile, Transform muzzleTransform)
        {
            projectile.transform.eulerAngles = muzzleTransform.eulerAngles * mode.SpawnRotationClamp;
            projectile.transform.parent = muzzleTransform;
            projectile.transform.localPosition = mode.ProjectileSpawnOffset;
            projectile.transform.parent = null;

            HandleAffix(mode, projectile);
        }
        /// <summary>
        /// Orients to the Rotation of the Muzzle with some Random Spread
        /// </summary>
        /// <param name="projectile"></param>
        private void OrientToMuzzleWithSpread(FireMode mode, Projectile projectile, Transform muzzleTransform)
        {
            projectile.transform.eulerAngles = muzzleTransform.eulerAngles * mode.SpawnRotationClamp
                                                        + new Vector3().RandomRange(-mode.Spread, mode.Spread);

            projectile.transform.parent = muzzleTransform;
            projectile.transform.localPosition = mode.ProjectileSpawnOffset;
            projectile.transform.parent = null;

            HandleAffix(mode, projectile);
        }
        /// <summary>
        /// Sets the angle of motion in increments, with optional hard lock to the orientation of the Muzzle
        /// </summary>
        /// <param name="projectile"></param>
        /// <param name="index"></param>
        private void OrientIncrementally(FireMode mode, Projectile projectile, int index, Transform muzzleTransform)
        {
            var euler = mode.AngularIncrement * index * !mode.IncrementalForcedAxes
                                                        + muzzleTransform.eulerAngles * mode.IncrementalForcedAxes * mode.SpawnRotationClamp;


            // For Incremental, rotating the Parent Muzzle Transform first ensures that the offset moves the Projectile in the correct direction,
            // locally. 
            projectile.transform.eulerAngles = euler;

            var previousEuler = muzzleTransform.eulerAngles;
            muzzleTransform.eulerAngles = euler;

            muzzleTransform.eulerAngles = projectile.transform.eulerAngles;

            projectile.transform.parent = muzzleTransform;
            projectile.transform.localPosition = mode.ProjectileSpawnOffset;
            projectile.transform.parent = null;

            muzzleTransform.eulerAngles = previousEuler;

            HandleAffix(mode, projectile);
        }
        /// <summary>
        /// Affixes the Instantiated Projectile to the Weapon Holder's Rigidbody if <see cref="Projectile.Fixed"/> is true.
        /// </summary>
        /// <param name="projectile"></param>
        private void HandleAffix(FireMode mode, Projectile projectile)
        {
            if (mode.Projectile.Data.Fixed)
            {
                FixedJoint joint = projectile.gameObject.AddComponent<FixedJoint>();
                joint.connectedBody = WeaponHolderRigidbody;
            }
        }

        public async IAsyncEnumerable<UniTaskVoid> Reload(int mode = -1)
        {
            if (!FireModeMuzzles[mode].FireMode.CanReload || FireModeMuzzles[mode].IsReloading) yield break;

            if (mode == -1)
            {
                for (int i = 0; i < FireModeMuzzles.Length; i++)
                {
                    FireModeMuzzles[i].IsReloading = true;
                    await UniTask.Delay(FireModeMuzzles[i].FireMode.ReloadTime);
                    FireModeMuzzles[i].AmmoCount = FireModeMuzzles[mode].FireMode.MaxAmmo;
                    FireModeMuzzles[i].IsReloading = false;
                }
            }
            else
            {
                FireModeMuzzles[mode].IsReloading = true;
                await UniTask.Delay(FireModeMuzzles[mode].FireMode.ReloadTime);
                FireModeMuzzles[mode].AmmoCount = FireModeMuzzles[mode].FireMode.MaxAmmo;
                FireModeMuzzles[mode].IsReloading = false;
            }
        }

        // Cancel the reload
        public void CancelReload(int mode)
        {
            if (FireModeMuzzles[mode].IsReloading)
            {
                FireModeMuzzles[mode].IsReloading = false; // Cancel reloading
            }
        }


        /// <summary>
        /// Weapon Handles are what are used to Translate/Orient Weapons relative to the Weapon Holder's available Sockets.
        /// </summary>
        [Serializable]
        public class WeaponHandle
        {
            public Vector3 Offset;
            [Tooltip("The world location of the Weapon Handle")]
            public Transform Transform;
            [Tooltip("The Socket that the Handle is placed in.")]
            public WeaponSocket Socket;
            [Tooltip("The Special ID of the socket for this Handle to be placed at.")]
            public int SpecialID = -1;
        }

        [Serializable]
        public class FireModeMuzzlePair
        {
            // TODO: Make these Customizable Events like the Weapon Input Events on the WeaponHolder.
            // This is vastly more modular. 

            /*        public ScriptableEvent OnReload;
                    public ScriptableEvent OnFire;*/

            public enum MuzzleSelectionStyle
            {
                All,
                Incremental,
                Random
            }
            [Tooltip("The method in which Muzzle Transform(s) is picked for the given relative FireMode.")]
            public MuzzleSelectionStyle MuzzleSelection;

            public bool InfiniteAmmo
            {
                get
                {
                    if (FireMode == null) return true;
                    else return FireMode.InfiniteAmmo;
                }
            }

            //[HideIf("InfiniteAmmo")]
            public int AmmoCount;
            [HideInInspector]
            public bool IsReloading;

            //[Dropdown("GetFiremodes")]
            public FireMode FireMode;
            public List<Transform> Muzzles = new();

            [HideInInspector]
            public WeaponData Data;
            /// <summary>
            /// The Index of the current Muzzle (only used when choosing a Muzzle Incrementally)
            /// </summary>
            [HideInInspector]
            public int MuzzleIndex = 0;

            /*public DropdownList<FireMode> GetFiremodes()
            {
                var list = new DropdownList<FireMode>();

                int i = 0;
                foreach (var fireMode in Data.FireModes)
                {
                    list.Add("Mode_" + i, fireMode);
                    i++;
                }

                return list;
            }*/
        }

        [Serializable]
        public class WeaponInputEvent
        {
            public SignalData OnInput;
            //[InfoBox("A FireMode value of -1 will trigger the event for all FireModes at once.")]
            public int FireMode = -1;
            [Tooltip("The Event fired when the event actually occurs")]
            public SignalData OnTriggered;
        }

        [Serializable]
        public class WeaponEvent
        {
            //[Dropdown("GetWeapons")]
            public WeaponData Weapon;
            public WeaponEventType EventType;
            public SignalData Event;

            public bool IsFireModeEvent => EventType == WeaponEventType.Fire || EventType == WeaponEventType.Reload;

/*            [Dropdown("GetFireModes")]
            [ShowIf("IsFireModeEvent")]*/
            public FireMode FireMode;

            [HideInInspector]
            public WeaponHolder Holder;

            /*public DropdownList<WeaponData> GetWeapons()
            {
                DropdownList<WeaponData> list = new();

                var weapons = Resources.LoadAll<WeaponData>("");

                if (weapons.Length == 0)
                    list.Add("No Weapons", null);
                else
                {
                    foreach (var weapon in weapons)
                    {
                        list.Add(weapon.name, weapon);
                    }
                }

                return list;
            }*/

/*
            public DropdownList<FireMode> GetFireModes()
            {
                DropdownList<FireMode> list = new();

                if (Weapon == null)
                    list.Add("Weapon Not Chosen", null);
                else
                {
                    int i = 0;
                    foreach (var mode in Weapon.FireModes)
                    {
                        list.Add("Mode_" + i, mode);
                        i++;
                    }
                }

                return list;
            }*/
        }


        public enum WeaponEventType
        {
            Fire,
            Reload,
            Equip,
            Unequip
        }

#if UNITY_EDITOR
        [MenuItem("Remedy/Weapons/Create Weapon")]
        public static void CreateWeaponPrefab()
        {
            string path = "Assets/Content/Prefabs/Weapons/New Weapon.prefab";

            // Create a new GameObject with a Weapon component
            GameObject weaponObject = new GameObject("NewWeapon");
            Weapon weapon = weaponObject.AddComponent<Weapon>();

            // Ensure the directory exists
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Create the prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(weaponObject, path);

            // Destroy the temporary GameObject in the scene
            GameObject.DestroyImmediate(weaponObject);

            Debug.Log("Weapon prefab created at: " + path, prefab);
        }
#endif
    }
}