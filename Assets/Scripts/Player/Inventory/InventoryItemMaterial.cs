using UnityEngine;
using VoxelTG.Terrain;

namespace VoxelTG.Player.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemMaterial_0", menuName = "Scriptable Objects/Inventory/Material")]
    public class InventoryItemMaterial : InventoryItemBase
    {
        [Header("Inventory item material settings")]
        /// <summary>
        /// Type of block stored in item
        /// </summary>
        public BlockType blockType = BlockType.AIR;

        public override bool IsMaterial()
        {
            return true;
        }

        public override bool IsSameType<T>(T other)
        {
            if(other is InventoryItemMaterial otherMaterial)
            {
                return otherMaterial.blockType == blockType;
            }
            
            return false;
        }
    }
}