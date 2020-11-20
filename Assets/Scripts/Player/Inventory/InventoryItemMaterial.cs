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

        /// <summary>
        /// Get item display name
        /// </summary>
        public override string ItemName
        {
            get
            {
                return blockType.ToString();
            }
        }
    }
}