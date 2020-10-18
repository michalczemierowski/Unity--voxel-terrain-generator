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
    public struct InventoryItemData
    {
        [NonSerialized]
        private Sprite itemIcon;

        public InventoryItemBase inventoryItem { get; private set; }
        /// <summary>
        /// slot position X in inventory
        /// </summary>
        public int positionX;
        /// <summary>
        /// slot position Y in inventory, toolbar = 0
        /// </summary>
        public int positionY;

        private int _itemCount;
        public int itemCount
        {
            get => _itemCount;
            set => _itemCount = math.clamp(value, 0, MaxStackSize);
        }

        public InventoryItemData(InventoryItemBase inventoryItem, int itemCount, int positionX, int positionY)
        {
            this.itemIcon = inventoryItem ? inventoryItem.itemIcon : null;
            this.inventoryItem = inventoryItem;
            this._itemCount = itemCount;
            this.positionX = positionX;
            this.positionY = positionY;
        }

        public InventoryItemData(ItemType toolType, int itemCount = 1)
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
        
        /// <summary>
        /// Check if slot is empty (no inventory item)
        /// </summary>
        public bool IsEmpty()
        {
            if (inventoryItem is InventoryItemMaterial material)
            {
                return material.blockType == BlockType.AIR;
            }
            else if (inventoryItem is InventoryItemTool tool)
            {
                return tool.itemType == ItemType.NONE;
            }
            else if(inventoryItem is InventoryItemWeapon weapon)
            {
                return weapon.itemType == ItemType.NONE;
            }

            return true;
        }

        /// <summary>
        /// Check if items in slots are the same
        /// </summary>
        /// <param name="data">InventoryItemData you want to compare</param>
        public bool IsSameItem(InventoryItemData data)
        {
            return data.BlockType == this.BlockType && data.ItemType == this.ItemType;
        }

        /// <summary>
        /// Check if item in slot is material
        /// </summary>
        public bool IsMaterial()
        {
            return (inventoryItem && inventoryItem is InventoryItemMaterial);
        }

        /// <summary>
        /// Check if item in slot is material
        /// </summary>
        /// <param name="inventoryItemMaterial">reference to material object</param>
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

        /// <summary>
        /// Check if item in slot is tool
        /// </summary>
        public bool IsTool()
        {
            return (inventoryItem && inventoryItem is InventoryItemTool);
        }

        /// <summary>
        /// Check if item in slot is tool
        /// </summary>
        /// <param name="inventoryItemTool">reference to tool object</param>
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

        /// <summary>
        /// Check if item in slot is weapon
        /// </summary>
        public bool IsWeapon()
        {
            return (inventoryItem && inventoryItem is InventoryItemWeapon);
        }

        /// <summary>
        /// Check if item in slot is weapon
        /// </summary>
        /// <param name="inventoryItemWeapon">reference to weapon object</param>
        public bool IsWeapon(out InventoryItemWeapon inventoryItemWeapon)
        {
            if (inventoryItem && inventoryItem is InventoryItemWeapon)
            {
                inventoryItemWeapon = inventoryItem as InventoryItemWeapon;
                return true;
            }

            inventoryItemWeapon = null;
            return false;
        }

        /// <summary>
        /// Get type of tool in slot, returns NONE if item is not Tool or Weapon
        /// </summary>
        public ItemType ItemType
        {
            get
            {
                if (inventoryItem)
                {
                    if (inventoryItem is InventoryItemTool itemTool)
                        return itemTool.itemType;
                    else if (inventoryItem is InventoryItemWeapon itemWeapon)
                        return itemWeapon.itemType;
                    else if(inventoryItem is InventoryItemMaterial itemMaterial)
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
                if (inventoryItem && inventoryItem is InventoryItemMaterial itemMaterial)
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
                if(inventoryItem && inventoryItem is InventoryItemTool itemTool)
                {
                    return itemTool.miningSpeed;
                }
                return 0;
            }
        }

        /// <summary>
        /// How many items can fit in one inventory slot
        /// </summary>
        public int MaxStackSize => inventoryItem ? inventoryItem.maxStack : 256;

        /// <summary>
        /// Get item inventory icon
        /// </summary>
        public Sprite ItemIcon => inventoryItem ? inventoryItem.itemIcon : null;

        /// <summary>
        /// Get item display name
        /// </summary>
        public string ItemName => inventoryItem ? inventoryItem.ItemName : string.Empty;

        /// <summary>
        /// empty InventoryItemData
        /// </summary>
        public static InventoryItemData EMPTY = new InventoryItemData(null, 0, 0, 0);
    }
}
