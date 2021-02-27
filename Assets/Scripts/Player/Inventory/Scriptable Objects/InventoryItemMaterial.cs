using UnityEngine;
using VoxelTG.Terrain;

namespace VoxelTG.Player.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemMaterial_0", menuName = "Scriptable Objects/Inventory/Material")]
    public class InventoryItemMaterial : InventoryItemBase
    {
        [Header("Material settings")]

        [Tooltip("Type of block stored in item")]
        [SerializeField] private BlockType blockType = BlockType.AIR;
        /// <summary>
        /// Type of block stored in item
        /// </summary>
        public BlockType BlockType => blockType;

        public override bool IsMaterial => true;

        public override bool IsSameType<T>(T other)
        {
            if(other is InventoryItemMaterial otherMaterial)
            {
                return otherMaterial.BlockType == BlockType;
            }
            
            return false;
        }
    }
}