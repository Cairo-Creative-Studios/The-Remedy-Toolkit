using Remedy.Framework;
/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;

namespace Remedy.Inventories
{
    [SchematicComponent("Inventory/Inventory")]
    //[Searchable]
    public class Inventory : MonoBehaviour
    {
        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Input")]*/
        public NavigationMode[] NavigationModes;
        //[InfoBox("The ID of InventoryItemData are passed to these Input Events to create a new InventoryItem instance when adding, or to remove the InventoryItem")]
        public SignalData SetItem;
        public SignalData AddItem;
        public SignalData RemoveItem;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Events", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Output")]*/
        public SignalData OnInventoryChanged;
        public SignalData OnSetActiveItem;
        public SignalData OnItemAdded;
        public SignalData OnItemRemoved;

        public int InventoryLayer => Data.InventoryLayer;

        /*[Layout("Settings", ELayout.Tab | ELayout.Collapse)]
        [Layout("Settings/Component", ELayout.Tab | ELayout.Collapse)]
        [LayoutStart("./Properties")]
        [Expandable]*/
        [SerializeField]
        private InventoryData _inventoryData;
        public InventoryData Data => _inventoryData ??= ScriptableObject.CreateInstance<InventoryData>();

        public InventoryItemCollection Contents => Data.Contents;

        // The ID of the Inventory Item Data in the Lookup
        private int _activeItemID;

        private void OnEnable()
        {
            // Ensure that the Inventory Data for this Inventory is unique by instantiating the Original
            if (Data != null)
                _inventoryData = Instantiate(Data);

            AddItem?.Subscribe(this, (int id) =>
            {
                Contents.Add(InventoryItemData.Lookup[id]);
            });

            RemoveItem?.Subscribe(this, (int id) =>
            {
                Contents.Remove(InventoryItemData.Lookup[id]);
            });

            SetItem?.Subscribe(this, (int id) =>
            {
                foreach (var item in Contents)
                {
                    if (item.ID == id)
                    {
                        SelectItem(_activeItemID);
                    }
                }
            });

            SelectItem(_activeItemID);

            // Subscribe switch Inputs
            /*foreach (var nav in NavigationModes)
            {
                switch (nav.Mode)
                {
                    case NavigationMode.NavigationLayout.Switch:
                        nav.SwitchInput?.Subscribe();
                            break;
                }
            }*/
        }

        private void OnDisable()
        {
            AddItem?.Unsubscribe(this);
            RemoveItem?.Unsubscribe(this);
        }

        void OnDestroy()
        {
            Destroy(Data);
            _inventoryData = null;
        }

        public void SelectItem(int itemIndex)
        {
            if (_activeItemID == itemIndex) return;

            if (_activeItemID > -1 && _activeItemID < Contents.Count)
                Contents[itemIndex]?.OnDeselected?.Invoke();

            if (itemIndex > -1 && itemIndex < Contents.Count)
            {
                _activeItemID = itemIndex;
                Contents[itemIndex]?.OnSelected?.Invoke();
            }
        }

        public class NavigationMode
        {
            public enum NavigationLayout
            {
                Switch, // Will switch to the next item in the Inventory and wrap around to the beginning
                SwitchDirectional, // Switches like before, but with the option to go backward
                Table // 2D Table Navigation for a kind of "Bag"
            }
            public NavigationLayout Mode;

            public bool IsSwitch => Mode == NavigationLayout.Switch;
            public bool IsSwitchDirection => Mode == NavigationLayout.SwitchDirectional;
            public bool IsTable => Mode == NavigationLayout.Table;

            //[ShowIf("IsSwitch")]
            public SignalData SwitchInput;
            //[ShowIf("IsSwitchDirection")]
            public SignalData DirectionalSwitchInput;
            //[ShowIf("IsTable")]
            public SignalData TableInput;
        }

        // Pick up 
        private void OnTriggerEnter(Collider other)
        {
            if (InventoryManager.GetInventoryLayer(other.gameObject) == Data.InventoryLayer)
            {
                var otherInventory = other.GetCachedComponent<CollectableInventory>();
                if (otherInventory != null)
                {
                    Data.Contents.TransferFrom(otherInventory.Items);
                    otherInventory.OnCollect?.Invoke(other.gameObject);
                }
            }
        }
    }
}