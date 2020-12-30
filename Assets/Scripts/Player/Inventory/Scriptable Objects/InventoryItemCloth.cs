using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemCloth_0", menuName = "Scriptable Objects/Inventory/Cloth")]
    public class InventoryItemCloth : InventoryItemBase
    {
        [Header("Inventory item cloth settings")]
        
        [Tooltip("Slot in which you can carry this item")]
        [SerializeField] private ClothingSlot clothingSlot;
        /// <summary>
        /// Slot in which you can carry this item
        /// </summary>
        public ClothingSlot ClothingSlot => clothingSlot;

        [Tooltip("How much armor will a player gain by wearing this item")]
        [SerializeField] private int armor;
        /// <summary>
        /// How much armor will a player gain by wearing this item
        /// </summary>
        public int Armor => armor;

        [SerializeField] private ClothingBuff[] clothingBuffs;
        public ClothingBuff[] ClothingBuffs => clothingBuffs;

        public override bool IsCloth => true;
    }
}