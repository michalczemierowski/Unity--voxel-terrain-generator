using UnityEngine;
using VoxelTG.Terrain;

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
        /// Type of item, NONE if there is no item
        /// </summary>
        public ItemType ItemType => Item ? Item.Type : ItemType.NONE;

        /// <summary>
        /// Type of block in slot, AIR if there is no item or item is not Material
        /// </summary>
        public BlockType BlockType
        {
            get
            {
                if (Item && Item is InventoryItemMaterial itemMaterial)
                {
                    return itemMaterial.BlockType;
                }
                return BlockType.AIR;
            }
        }

        /// <summary>
        /// Weight of all units in inventory
        /// </summary>
        public int ItemWeight => Item ? Item.Weight * ItemAmount : 0;

        /// <summary>
        /// Item icon
        /// </summary>
        public Sprite ItemIcon => Item ? Item.Icon : null;

        /// <summary>
        /// Item display name
        /// </summary>
        public string ItemName => Item ? Item.Name : string.Empty;

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
