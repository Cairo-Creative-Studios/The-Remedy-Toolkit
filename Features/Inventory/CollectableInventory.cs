using Remedy.Framework;
/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;
using Remedy.Common;

namespace Remedy.Inventories
{
    [SchematicComponent("Inventory/Collectable")]
    //[Searchable]
    [RequireComponent(typeof(SphereCollider))]
    public class CollectableInventory : MonoBehaviour
    {
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        [Tooltip("Called when this Inventory is Collected, passing the GameObject that contains the Inventory Component that picked it up.")]
        public SignalData OnCollect;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]*/
        public InventoryItemCollection Items = new();
        //[InfoBox("If this is set, the contents of CollectionData will copy into the Items of this collectable.")]
        public CollectableInventoryData CollectionData;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Variables")]*/
        public GameObject RenderedObject;

        private void OnEnable()
        {
            var pickupCollider = GetComponent<SphereCollider>();
            pickupCollider.isTrigger = true;
            pickupCollider.radius = GlobalInventoryData.Instance.ItemPickupRange;

            if (CollectionData != null)
                Items = CollectionData.Contents;

            if (CollectionData.RenderAsItem)
            {
                RenderedObject = Instantiate(CollectionData.Contents[0].Prefab);
            }
        }

        private void OnDisable()
        {
            Destroy(RenderedObject);
        }

        private void OnValidate()
        {
            var pickupCollider = GetComponent<SphereCollider>();
            pickupCollider.isTrigger = true;
            pickupCollider.radius = GlobalInventoryData.Instance.ItemPickupRange;
        }
    }
}