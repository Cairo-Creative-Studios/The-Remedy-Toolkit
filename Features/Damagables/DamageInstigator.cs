using Remedy.Framework;
using Remedy.Schematics.Utils;
/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.Damagables
{
    [SchematicComponent("Damageables/Instigator")]
    ///[Searchable]
    public class DamageInstigator : MonoBehaviour
    {
/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Input")]*/
        public SignalData Instigate;
/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        public SignalData OnInstigation;

/*        [Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]*/
        public DamageInstigationData Data;

        void OnEnable()
        {
            Instigate?.Subscribe(this, (Union value) =>
            {
                DamageManager.InstigateDamage(new(transform.position, Data.ID));
            });
        }
        void OnDisable()
        {
            Instigate?.Unsubscribe(this);
        }

/*        public DropdownList<DamageType> GetDamageTypes()
        {
            DropdownList<DamageType> list = new();
            var damageTypes = Resources.LoadAll<DamageType>("");
            if (damageTypes.Length == 0)
                list.Add("No Damage Types", null);
            else
            {
                foreach(var damageType in damageTypes)
                {
                    list.Add(damageType.name, damageType);
                }
            }
            return list;
        }*/

        private void OnTriggerStay(Collider other)
        {
            if (Data.Instigation != DamageInstigationData.InstigationType.OnTriggerStay) return;
            var damageable = other.GetCachedComponent<Damageable>();

            if(damageable != null && damageable.DamageLayer == Data.DamageLayer)
            {
                damageable.Damage(new(transform.position, Data.ID));
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Data.Instigation != DamageInstigationData.InstigationType.OnCollisionEnter) return;
            var damageable = collision.gameObject.GetCachedComponent<Damageable>();

            if (damageable != null && damageable.DamageLayer == Data.DamageLayer)
            {
                damageable.Damage(new(transform.position, Data.ID));
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (Data.Instigation != DamageInstigationData.InstigationType.OnCollisionEnter) return;
            var damageable = other.GetCachedComponent<Damageable>();

            if (damageable != null && damageable.DamageLayer == Data.DamageLayer)
            {
                damageable.Damage(new(transform.position, Data.ID));
            }
        }
    }
}
