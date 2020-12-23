using UnityEngine;

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
        /// <summary>
        /// Item display name
        /// </summary>
        public string ItemName;

        /// <summary>
        /// How many items can fit in one inventory slot
        /// </summary>
        public int maxStack = 256;
        /// <summary>
        /// Sprite that will be displayed as item icon in inventory
        /// </summary>
        public Sprite itemIcon;

        public virtual bool IsMaterial()
        {
            return false;
        }

        public virtual bool IsTool()
        {
            return false;
        }

        public virtual bool IsWeapon()
        {
            return false;
        }

        public virtual bool IsSameType<T>(T other) where T : InventoryItemBase
        {
            return false;
        }
    }
}
