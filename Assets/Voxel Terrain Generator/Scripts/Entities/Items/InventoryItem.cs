/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;

namespace VoxelTG.Player.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItem_0", menuName = "Scriptable Objects/InventoryItem")]
    public class InventoryItem : ScriptableObject
    {
        [Header("Inventory item settings")]
        public int maxStack = 256;
        public Sprite itemIcon;

        public static InventoryItem EMPTY => new InventoryItem() { maxStack = 0, itemIcon = null};
    }

    [CreateAssetMenu(fileName = "InventoryItemTool_0", menuName = "Scriptable Objects/InventoryItemTool")]
    public class InventoryItemTool : InventoryItem
    {
        [Header("Inventory item tool settings")]
        public ToolType toolType = ToolType.NONE;
        public int durability;
        public float damage;

        public string addressablePathToModel;
    }

    [CreateAssetMenu(fileName = "InventoryItemMaterial_0", menuName = "Scriptable Objects/InventoryItemMaterial")]
    public class InventoryItemMaterial : InventoryItem
    {
        [Header("Inventory item material settings")]
        public BlockType blockType = BlockType.AIR;
    }
}
