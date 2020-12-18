using System;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        public InventoryItemBase Item { get; private set; }
        /// <summary>
        /// slot position X in inventory
        /// </summary>
        public readonly int PositionX;
        /// <summary>
        /// slot position Y in inventory, toolbar = 0
        /// </summary>
        public readonly int PositionY;

        private int _itemCount;
        public int ItemCount
        {
            get => _itemCount;
            set => _itemCount = math.clamp(value, 0, MaxStackSize);
        }

        public InventorySlot(InventoryItemBase inventoryItem, int itemCount, int positionX, int positionY)
        {
            this.Item = inventoryItem;
            this._itemCount = itemCount;
            this.PositionX = positionX;
            this.PositionY = positionY;
        }

        public InventorySlot(int positionX, int positionY)
        {
            this.PositionX = positionX;
            this.PositionY = positionY;
        }
        
        /// <summary>
        /// Check if slot is empty (no inventory item)
        /// </summary>
        public bool IsEmpty()
        {
            return Item == null;
        }

        /// <summary>
        /// Get type of tool in slot, returns NONE if item is not Tool or Weapon
        /// </summary>
        public ItemType ItemType
        {
            get
            {
                if (Item)
                {
                    if (Item is InventoryItemTool itemTool)
                        return itemTool.itemType;
                    else if (Item is InventoryItemWeapon itemWeapon)
                        return itemWeapon.itemType;
                    else if(Item is InventoryItemMaterial itemMaterial)
                        return ItemType.MATERIAL;
                }
 
                return ItemType.NONE;
            }
        }

        /// <summary>
        /// Get type of block in slot, returns AIR if item is not Material
        /// </summary>
        public BlockType BlockType
        {
            get
            {
                if (Item && Item is InventoryItemMaterial itemMaterial)
                {
                    return itemMaterial.blockType;
                }
                return BlockType.AIR;
            }
        }

        /// <summary>
        /// Get mining speed, return 0 if item is not tool
        /// </summary>
        public float MiningSpeed
        {
            get
            {
                if(Item && Item is InventoryItemTool itemTool)
                {
                    return itemTool.miningSpeed;
                }
                
                return 0;
            }
        }

        /// <summary>
        /// How many items can fit in one inventory slot
        /// </summary>
        public int MaxStackSize => Item ? Item.maxStack : 1;

        /// <summary>
        /// Get item inventory icon
        /// </summary>
        public Sprite ItemIcon => Item ? Item.itemIcon : null;

        /// <summary>
        /// Get item display name
        /// </summary>
        public string ItemName => Item ? Item.ItemName : string.Empty;

        /// <summary>
        /// Remove item from slot
        /// </summary>
        public void ClearSlot()
        {
            Item = null;
            ItemCount = 0;
        }
    }
}
