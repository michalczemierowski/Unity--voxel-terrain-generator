using UnityEngine;
using UnityEngine.AddressableAssets;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemTool_0", menuName = "Scriptable Objects/Inventory/Tool")]
    public class InventoryItemTool : InventoryItemBase
    {
        [Header("Tool settings")]

        [Tooltip("How many times you can use a tool before destroying it")]
        [SerializeField] private int durability = 64;
        /// <summary>
        /// How many times you can use a tool before destroying it
        /// </summary>
        public int Durability => durability;

        [SerializeField] private float miningSpeed = 1;
        /// <summary>
        /// Multiplier of time needed to destroy the block (lower = faster)
        /// </summary>
        public float MiningSpeed => miningSpeed;

        public override bool IsTool => true;

        #region // === Metadata keys === \\

        /// <summary>
        /// Metadata key that is used to store tool's durability
        /// </summary>
        public const string DURABILITY_MDK = "durability";

        #endregion
    }
}