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
        public InventoryItemBase Item { get; }

        private int _itemAmount;
        public int ItemAmount
        {
            get => _itemAmount;
            set
            {
                _itemAmount = value > 0 ? value : 1;

                OnAmountUpdate?.Invoke(_itemAmount, ItemWeight);
            }
        }

        public InventorySlot(InventoryItemBase inventoryItem, int amount)
        {
            this.Item = inventoryItem;
            this._itemAmount = amount;
        }

        /// <summary>
        /// Check if slot is empty (inventory item is null)
        /// </summary>
        public bool IsEmpty()
        {
            return Item == null;
        }

        /// <summary>
        /// Type of tool in slot, NONE if item is not Tool or Weapon
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
                    else if (Item is InventoryItemMaterial)
                        return ItemType.MATERIAL;
                }

                return ItemType.NONE;
            }
        }

        /// <summary>
        /// Type of block in slot, AIR if item is not Material
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
        /// Mining speed, 0 if item is not tool
        /// </summary>
        public float MiningSpeed
        {
            get
            {
                if (Item && Item is InventoryItemTool itemTool)
                {
                    return itemTool.miningSpeed;
                }

                return 0;
            }
        }

        /// <summary>
        /// Weight of all units in inventory
        /// </summary>
        public int ItemWeight => Item ? Item.weight * ItemAmount : 0;

        /// <summary>
        /// Item icon
        /// </summary>
        public Sprite ItemIcon => Item ? Item.itemIcon : null;

        /// <summary>
        /// Item display name
        /// </summary>
        public string ItemName => Item ? Item.ItemName : string.Empty;

        public void InvokeDestroyEvent()
        {
            OnSlotRemoved?.Invoke();
        }

        #region // === Events === \\

        public delegate void AmountUpdate(int newAmount, int newWeight);
        /// <summary>
        /// Called when item amount changes
        /// </summary>
        public AmountUpdate OnAmountUpdate;

        public delegate void SlotRemoved();
        /// <summary>
        /// Called when slot is to be destroyed (e.g. item dropped on ground)
        /// </summary>
        public SlotRemoved OnSlotRemoved;

        #endregion
    }
}
