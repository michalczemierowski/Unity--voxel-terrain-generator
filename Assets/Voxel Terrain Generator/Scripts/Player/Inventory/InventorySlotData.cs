/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;
using VoxelTG.UI;

namespace VoxelTG.Player.Inventory
{
    [System.Serializable]
    public struct InventoryItemData
    {
        [System.NonSerialized]
        private Sprite itemIcon;

        public InventoryItem inventoryItem { get; private set; }
        public int positionX;
        public int positionY;

        private int _itemCount;
        public int itemCount
        {
            get => _itemCount;
            set => _itemCount = math.clamp(value, 0, MaxStackSize);
        }

        public InventoryItemData(InventoryItem inventoryItem, int itemCount, int positionX, int positionY)
        {
            this.itemIcon = inventoryItem ? inventoryItem.itemIcon : null;
            this.inventoryItem = inventoryItem;
            this._itemCount = itemCount;
            this.positionX = positionX;
            this.positionY = positionY;
        }

        public InventoryItemData(ToolType toolType, int itemCount = 1)
        {
            this.inventoryItem = UIManager.Instance.inventoryUI.GetToolItemData(toolType);
            this.itemIcon = inventoryItem ? inventoryItem.itemIcon : null;
            this._itemCount = itemCount;
            this.positionX = 0;
            this.positionY = 0;
        }

        public InventoryItemData(BlockType blockType, int itemCount = 1)
        {
            this.inventoryItem = UIManager.Instance.inventoryUI.GetMaterialItemData(blockType);
            this.itemIcon = inventoryItem ? inventoryItem.itemIcon : null;
            this._itemCount = itemCount;
            this.positionX = 0;
            this.positionY = 0;
        }

        public bool IsEmpty()
        {
            if (inventoryItem is InventoryItemMaterial material)
            {
                return material.blockType == BlockType.AIR;
            }
            else if (inventoryItem is InventoryItemTool tool)
            {
                return tool.toolType == ToolType.NONE;
            }

            return true;
        }

        public bool IsMaterial()
        {
            if (inventoryItem && inventoryItem is InventoryItemMaterial)
                return true;

            return false;
        }

        public bool IsMaterial(out InventoryItemMaterial inventoryItemMaterial)
        {
            if (inventoryItem && inventoryItem is InventoryItemMaterial)
            {
                inventoryItemMaterial = inventoryItem as InventoryItemMaterial;
                return true;
            }

            inventoryItemMaterial = null;
            return false;
        }

        public bool IsTool()
        {
            if (inventoryItem && inventoryItem is InventoryItemTool)
                return true;

            return false;
        }

        public bool IsTool(out InventoryItemTool inventoryItemTool)
        {
            if (inventoryItem && inventoryItem is InventoryItemTool)
            {
                inventoryItemTool = inventoryItem as InventoryItemTool;
                return true;
            }

            inventoryItemTool = null;
            return false;
        }

        public ToolType ToolType
        {
            get
            {
                if (inventoryItem && inventoryItem is InventoryItemTool itemTool)
                {
                    return itemTool.toolType;
                }
                return ToolType.NONE;
            }
        }//=> inventoryItem ? inventoryItem.toolType : ToolType.NONE;

        public BlockType BlockType
        {
            get
            {
                if (inventoryItem && inventoryItem is InventoryItemMaterial itemMaterial)
                {
                    return itemMaterial.blockType;
                }
                return BlockType.AIR;
            }
        }// => inventoryItem ? inventoryItem.blockType : BlockType.AIR;

        public int MaxStackSize => inventoryItem ? inventoryItem.maxStack : 256;

        public Sprite ItemIcon => inventoryItem ? inventoryItem.itemIcon : null;

        public static InventoryItemData EMPTY = new InventoryItemData(null, 0, 0, 0);
    }
}
