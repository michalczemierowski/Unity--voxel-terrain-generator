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
        [Tooltip("Item display name")]
        public string ItemName;

        /// <summary>
        /// Weight of signle unit in inventory
        /// </summary>
        [Tooltip("Weight of signle unit in inventory")]
        public int weight = 1;

        /// <summary>
        /// Sprite that will be displayed as item icon in inventory
        /// </summary>
        [Tooltip("Icon that will be displayed as item icon")]
        public Sprite itemIcon;

        /// <summary>
        /// Check if item is material
        /// </summary>
        /// <returns>false by default</returns>
        public virtual bool IsMaterial()
        {
            return false;
        }

        /// <summary>
        /// Check if item is material
        /// </summary>
        /// <returns>false by default</returns>
        public virtual bool IsTool()
        {
            return false;
        }

        /// <summary>
        /// Check if item is material
        /// </summary>
        /// <returns>false by default</returns>
        public virtual bool IsWeapon()
        {
            return false;
        }

        /// <summary>
        /// Check if items are same type
        /// </summary>
        /// <typeparam name="T">type of items to compare (must be child of InventoryItemBase)</typeparam>
        /// <param name="other">item to compare</param>
        /// <returns>false by default</returns>
        public virtual bool IsSameType<T>(T other) where T : InventoryItemBase
        {
            return false;
        }
    }
}
