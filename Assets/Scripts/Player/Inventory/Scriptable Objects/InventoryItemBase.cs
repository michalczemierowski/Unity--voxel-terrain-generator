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

        [Tooltip("Item display name")]
        [SerializeField] private string itemName;
        /// <summary>
        /// Item display name
        /// </summary>
        public string Name => itemName;

        [Tooltip("Weight of signle unit in inventory")]
        [SerializeField] private int weight = 1;
        /// <summary>
        /// Weight of signle unit in inventory
        /// </summary>
        public int Weight => weight;

        [Tooltip("Icon that will be displayed as item icon")]
        [SerializeField] private Sprite itemIcon;
        /// <summary>
        /// Sprite that is displayed as item icon in inventory
        /// </summary>
        public Sprite Icon => itemIcon;

        [Tooltip("Type of item")]
        [SerializeField] private ItemType itemType;
        /// <summary>
        /// Type of item
        /// </summary>
        public ItemType Type => itemType;

        [Tooltip("Group to which item belongs")]
        [SerializeField] private ItemGroup itemGroup;
        /// <summary>
        /// Group to which item belongs
        /// </summary>
        public ItemGroup Group => itemGroup;

        /// <summary>
        /// Check if item is material
        /// </summary>
        public virtual bool IsMaterial => false;

        /// <summary>
        /// Check if item is material
        /// </summary>
        public virtual bool IsTool => false;

        /// <summary>
        /// Check if item is material
        /// </summary>
        public virtual bool IsWeapon => false;

        /// <summary>
        /// Check if item is cloth
        /// </summary>
        public virtual bool IsCloth => false;

        /// <summary>
        /// Check if items are same type
        /// </summary>
        /// <typeparam name="T">type of items to compare (must be child of InventoryItemBase)</typeparam>
        /// <param name="other">item to compare</param>
        /// <returns>compare Types by default</returns>
        public virtual bool IsSameType<T>(T other) where T : InventoryItemBase
        {
            return other.Type == Type;;
        }
    }
}
