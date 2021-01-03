using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<string, IMetadataProperty> slotMetadata;

        public InventorySlot(InventoryItemBase inventoryItem, int amount)
        {
            this.Item = inventoryItem;
            this._itemAmount = amount;

            slotMetadata = new Dictionary<string, IMetadataProperty>();

            // don't check metadata if item is empty
            if (!IsEmpty())
            {
                // read tool metadata
                if (Item.IsTool)
                {
                    var tool = (InventoryItemTool)Item;

                    MetadataProperty<int> durabilityMeta = new MetadataProperty<int>(InventoryItemTool.DURABILITY_MDK, tool.Durability);
                    SetMetadata(durabilityMeta);
                }
                // read weapon metadata
                else if (Item.IsWeapon)
                {
                    var weapon = (InventoryItemWeapon)Item;

                    MetadataProperty<int> clipMeta = new MetadataProperty<int>(InventoryItemWeapon.AMMO_CURRENT_CLIP_MDK, weapon.ClipSize);
                    SetMetadata(clipMeta);
                }
            }
        }

        /// <summary>
        /// Set slot metadata
        /// </summary>
        public void SetMetadata(IMetadataProperty data)
        {
            if (System.String.IsNullOrEmpty(data.Key))
            {
                Debug.LogError("Key is null or empty - InventorySlot.SetMetadata(IMetadataProperty data)");
                return;
            }

            slotMetadata[data.Key] = data;
            OnMetadataUpdate?.Invoke(data.Key, data);
        }

        /// <summary>
        /// Try to get metadata by key
        /// </summary>
        /// <param name="key">key to data</param>
        /// <param name="data">out metadata property</param>
        /// <returns>true if property found</returns>
        public bool TryGetMedadata(string key, out IMetadataProperty data)
        {
            if (System.String.IsNullOrEmpty(key))
            {
                Debug.LogError("Key is null or empty - InventorySlot.TryGetMedadata(string key, out IMetadataProperty data). Returning empty metadata with bool value");
                data = new MetadataProperty<bool>();
                return false;
            }

            return slotMetadata.TryGetValue(key, out data);
        }

        /// <summary>
        /// Try to get specific metadata property by key and type
        /// </summary>
        /// <param name="key">key to data</param>
        /// <param name="data">out metadata property</param>
        /// <typeparam name="T">type of object stored in property</typeparam>
        /// <returns>true if property found</returns>
        public bool TryGetMedadata<T>(string key, out MetadataProperty<T> data) where T : unmanaged
        {
            if(TryGetMedadata(key, out IMetadataProperty metadata) && metadata is MetadataProperty<T> metaProperty)
            {
                data = metaProperty;
                return true;
            }

            data = new MetadataProperty<T>();
            return false;
        }

        /// <summary>
        /// Get all metadata keys and values
        /// </summary>
        public IMetadataProperty[] GetAllMetadataProperties()
        {
            return slotMetadata.Values.ToArray();
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

        /// <summary>
        /// This method should be called before destroying object
        /// </summary>
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

        public delegate void MetadataUpdate(string key, IMetadataProperty newData);
        /// <summary>
        /// Called when any value in medatata changes
        /// </summary>
        public MetadataUpdate OnMetadataUpdate;

        #endregion
    }
}
