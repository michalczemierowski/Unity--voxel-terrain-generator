using UnityEngine;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItem_0", menuName = "Scriptable Objects/Inventory/Item")]
    /// <summary>
    /// Base object for inventory items
    /// </summary>
    public class InventoryItemBase : ScriptableObject
    {
        [Header("Inventory item settings")]
        [SerializeField] private string itemName;

        /// <summary>
        /// How many items can fit in one inventory slot
        /// </summary>
        public int maxStack = 256;
        /// <summary>
        /// Sprite that will be displayed as item icon in inventory
        /// </summary>
        public Sprite itemIcon;

        public static InventoryItemBase EMPTY => new InventoryItemBase() { maxStack = 0, itemIcon = null};

        /// <summary>
        /// Item display name
        /// </summary>
        public virtual string ItemName
        {
            get
            {
                return itemName;
            }
        }

    }
}
