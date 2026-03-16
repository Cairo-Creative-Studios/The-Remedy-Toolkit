/*using SaintsField;
using SaintsField.Playa;*/
using UnityEngine;
using Remedy.Common;

namespace Remedy.Inventories
{
    [CreateAssetMenu(menuName = "Remedy Toolkit/Inventory/Default Item Data")]
    //[Searchable]
    public class InventoryItemData : ScriptableObjectWithID<InventoryItemData>
    {
        [Tooltip("Invoked when the Item is selected by an Inventory.")]
        public SignalData OnSelected;
        [Tooltip("Invoked when the Item is de-selected by an Inventory.")]
        public SignalData OnDeselected;

        [Tooltip("If displayed in the world, this Prefab will be created for it.")]
        public GameObject Prefab;
        [Tooltip("An optional Sprite that can be used to display this in the inventory.")]
        public Sprite Sprite;
        //[Dropdown("GetInventoryLayer")]
        public int InventoryLayer;
        //public DropdownList<int> GetInventoryLayer => GlobalInventoryData.GetInventoryLayers();
    }
}