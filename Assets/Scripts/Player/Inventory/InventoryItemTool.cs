using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemTool_0", menuName = "Scriptable Objects/Inventory/Tool")]
    public class InventoryItemTool : InventoryItemBase
    {
        [Header("Inventory item tool settings")]
        /// <summary>
        /// How many block can tool mine before being destroyed
        /// </summary>
        public int durability;
        /// <summary>
        /// Time required to destroy block multipler, more miningSpeed = faster mining
        /// </summary>
        public float miningSpeed;

        /// <summary>
        /// Path to in-hand model
        /// </summary>
        public string addressablePathToModel;

        public override bool IsTool()
        {
            return true;
        }
    }
}