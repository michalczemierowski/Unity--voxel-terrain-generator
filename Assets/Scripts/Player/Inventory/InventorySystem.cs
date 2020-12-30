using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VoxelTG.Entities.Items;
using VoxelTG.Extensions;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    public class InventorySystem : MonoBehaviour
    {
        #region // === Variables === \\

        private const string PATH_TO_ITEMS_DATA = "inventory/items/";
        private const string PATH_TO_MATERIAL_DATA = "inventory/materials/";

        [SerializeField] private int carryingCapacity;

        public InventorySlot HandSlot { get; private set; }
        public int CarryingCapacity => carryingCapacity;

        private int occupiedCarringCapacity;
        public int OccupiedCarringCapacity
        {
            get => occupiedCarringCapacity;
            private set
            {
                // if player is gonna be overloaded
                if (value > carryingCapacity)
                {
                    // if player wasn't overloaded before
                    if (occupiedCarringCapacity <= carryingCapacity)
                    {
                        DebugUtils.DebugConsole.AddDebugMessageStatic("YOU'RE OVERLOADED");
                    }

                    IsOverloaded = true;
                }
                else
                {
                    // if player was overloaded before
                    if(occupiedCarringCapacity > carryingCapacity)
                    {
                        DebugUtils.DebugConsole.AddDebugMessageStatic("YOU'RE NO LONGER OVERLOADED");
                    }

                    IsOverloaded = false;
                }

                // occupied capacity can't be lower than 0
                occupiedCarringCapacity = value > 0 ? value : 0;
            }
        }


        public bool IsOverloaded { get; private set; }
        public bool IsHandNullOrEmpty => HandSlot.IsNullOrEmpty();

        /// <summary>
        /// Array containing all inventory slots
        /// </summary>
        public HashSet<InventorySlot> InventorySlots { get; private set; }

        private Dictionary<ItemType, InventoryItemBase> inventoryItemDataCache;
        private Dictionary<BlockType, InventoryItemBase> inventoryMaterialDataCache;

        #endregion

        #region // === Events === \\

        public delegate void MainHandUpdate(InventorySlot oldContent, InventorySlot newContent);
        /// <summary>
        /// Called when InventorySlot in main hand changes
        /// </summary>
        public MainHandUpdate OnMainHandUpdate;

        public delegate void InventoryContentsChange(HashSet<InventorySlot> inventorySlots, bool onlyAmountChanged);
        /// <summary>
        /// Called when new item is added/removed or amount of any item changes
        /// </summary>
        public InventoryContentsChange OnInventoryContentsChange;

        #endregion

        private void OnInventoryContentsChangePreProcess(bool onlyAmountChanged)
        {
            int occupiedCapacity = 0;
            foreach (var slot in InventorySlots)
            {
                occupiedCapacity += slot.ItemWeight;
            }

            OccupiedCarringCapacity = occupiedCapacity;
        }

        private void Awake()
        {
            InventorySlots = new HashSet<InventorySlot>();
            inventoryItemDataCache = new Dictionary<ItemType, InventoryItemBase>();
            inventoryMaterialDataCache = new Dictionary<BlockType, InventoryItemBase>();

            HandSlot = new InventorySlot(null, 0);
        }

        /// <summary>
        /// Load and cache item data for provided item type.
        /// </summary>
        /// <param name="itemType">type of item</param>
        /// <param name="onLoadingCompleted">action that will be called when loading is complete</param>
        private void LoadItemData(ItemType itemType, System.Action<InventoryItemBase> onLoadingCompleted = null)
        {
            // return if data is already in cache
            if (inventoryItemDataCache.ContainsKey(itemType))
            {
                Debug.LogError($"Item data for {itemType} is already cached. You should use GetItemData method instead of LoadItemData.", this);
                return;
            }

            string path = PATH_TO_ITEMS_DATA + itemType.ToString() + ".asset";
            Addressables.LoadAssetAsync<InventoryItemBase>(path).Completed += (handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Failed to load item data for {itemType} ({path})", this);
                    return;
                }

                // add data to cache
                inventoryItemDataCache.Add(itemType, handle.Result);
                onLoadingCompleted?.Invoke(handle.Result);
            };
        }

        /// <summary>
        /// Load and cache item data for provided item type. (use <see cref="GetItemData"/> if you want to check if object is cached)
        /// </summary>
        /// <param name="blockType">type of item</param>
        /// <param name="onLoadingCompleted">action that will be called when loading is complete</param>
        private void LoadItemData(BlockType blockType, System.Action<InventoryItemBase> onLoadingCompleted = null)
        {
            // return if data is already in cache
            if (inventoryMaterialDataCache.ContainsKey(blockType))
            {
                Debug.LogError($"Item data for {blockType} is already cached. You should use GetItemData method instead of LoadItemData.", this);
                return;
            }

            string path = PATH_TO_MATERIAL_DATA + blockType.ToString() + ".asset";
            Addressables.LoadAssetAsync<InventoryItemBase>(path).Completed += (handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"Failed to load item data for {blockType} ({path})");
                    return;
                }

                // add data to cache
                inventoryMaterialDataCache.Add(blockType, handle.Result);
                onLoadingCompleted?.Invoke(handle.Result);
            };
        }

        /// <summary>
        /// Get cached item data or load data from file system and call action
        /// </summary>
        /// <param name="itemType">type of item</param>
        /// <param name="action">action that will be called if item data was found</param>
        public void GetItemData(ItemType itemType, System.Action<InventoryItemBase> action)
        {
            if (itemType == ItemType.NONE || action == null)
                return;

            // item already in cache
            if (inventoryItemDataCache.TryGetValue(itemType, out var item))
                action.Invoke(item);
            // else - load data from file system
            else
                LoadItemData(itemType, action);
        }

        /// <summary>
        /// Get cached item data or load data from file system and call action
        /// </summary>
        /// <param name="blockType">type of item</param>
        /// <param name="action">action that will be called if item data was found</param>
        public void GetItemData(BlockType blockType, System.Action<InventoryItemBase> action)
        {
            if (blockType == BlockType.AIR || action == null)
                return;

            // item already in cache
            if (inventoryMaterialDataCache.TryGetValue(blockType, out var item))
                action.Invoke(item);
            // else - load data from file system
            else
                LoadItemData(blockType, action);
        }

        /// <summary>
        /// Add 'amount' of 'inventoryItem' to inventory
        /// </summary>
        /// <param name="inventoryItem">inventory item which you want to add</param>
        /// <param name="amount">amount of item</param>
        public void AddItem(InventoryItemBase inventoryItem, int amount)
        {
            if (amount < 1 || inventoryItem == null)
                return;

            // check if containing this type of item exists
            InventorySlot existingSlot = InventorySlots.FirstOrDefault((item) => item.ItemName.Equals(inventoryItem.Name));
            // if so, just update amount
            if (existingSlot != null)
            {
                existingSlot.ItemAmount += amount;
                OnInventoryContentsChangePreProcess(true);
                OnInventoryContentsChange?.Invoke(InventorySlots, true);
            }
            // else, create new slot
            else
            {
                InventorySlot newSlot = new InventorySlot(inventoryItem, amount);
                InventorySlots.Add(newSlot);
                OnInventoryContentsChangePreProcess(false);
                OnInventoryContentsChange?.Invoke(InventorySlots, false);
            }
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        /// <param name="itemType">type of item</param>
        /// <param name="amount">item amount</param>
        public void AddItem(ItemType itemType, int amount)
        {
            if (amount < 1 || itemType == ItemType.NONE)
                return;

            // item already in cache
            if (inventoryItemDataCache.TryGetValue(itemType, out var item))
                AddItem(item, amount);
            // else - load data from file system
            else
                LoadItemData(itemType, (result) => AddItem(result, amount));
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        /// <param name="blockType">type of item</param>
        /// <param name="amount">item amount</param>
        public void AddItem(BlockType blockType, int amount)
        {
            if (amount < 1 || blockType == BlockType.AIR)
                return;

            // item already in cache
            if (inventoryMaterialDataCache.TryGetValue(blockType, out var item))
                AddItem(item, amount);
            // else - load data from file system
            else
                LoadItemData(blockType, (result) => AddItem(result, amount));
        }

        /// <summary>
        /// Decrease amount of 'inventoryItem' by 'amount'
        /// </summary>
        /// <param name="inventoryItem">type of item</param>
        /// <param name="amount">amount to remove</param>
        /// <param name="existingSlot">inventory slot in which you want to decrease item amount (will try to find one if null)</param>
        /// <returns>removed items count</returns>
        public int RemoveItem(InventoryItemBase inventoryItem, int amount = 1, InventorySlot existingSlot = null)
        {
            if (amount == 0)
                return 0;

            if (existingSlot == null)
                existingSlot = InventorySlots.FirstOrDefault((item) => item.ItemName.Equals(inventoryItem.Name));

            if (existingSlot != null)
            {
                // if amount < 0 - remove item from inventory
                if (amount < 0)
                {
                    InventorySlots.Remove(existingSlot);
                    SetInHandSlot(null);
                    OnInventoryContentsChangePreProcess(false);
                    OnInventoryContentsChange?.Invoke(InventorySlots, false);

                    return existingSlot.ItemAmount;
                }
                // else substract toRemove from count
                else
                {
                    int toRemove = Mathf.Min(existingSlot.ItemAmount, amount);
                    int amountLeft = existingSlot.ItemAmount - toRemove;
                    if (amountLeft < 1)
                    {
                        existingSlot.InvokeDestroyEvent();
                        InventorySlots.Remove(existingSlot);
                        SetInHandSlot(null);
                        OnInventoryContentsChangePreProcess(false);
                        OnInventoryContentsChange?.Invoke(InventorySlots, false);
                    }
                    else
                    {
                        existingSlot.ItemAmount = amountLeft;
                        OnInventoryContentsChangePreProcess(true);
                        OnInventoryContentsChange?.Invoke(InventorySlots, true);
                    }

                    return toRemove;
                }
            }

            return 0;
        }

        /// <summary>
        /// Decrease amount of item 'itemType' by 'amount'
        /// </summary>
        /// <param name="blockType">type of item</param>
        /// <param name="amount">amount to remove</param>
        /// <returns>removed items count</returns>
        public int RemoveItem(BlockType blockType, int amount = 1)
        {
            if (amount == 0 || blockType == BlockType.AIR)
                return 0;

            // remove item if slot is found
            if (inventoryMaterialDataCache.TryGetValue(blockType, out var inventoryItem))
                return RemoveItem(inventoryItem, amount);

            return 0;
        }

        /// <summary>
        /// Decrease amount of item 'itemType' by 'amount'
        /// </summary>
        /// <param name="itemType">type of item</param>
        /// <param name="amount">amount to remove</param>
        /// <returns>removed items count</returns>
        public int RemoveItem(ItemType itemType, int amount = 1)
        {
            if (amount == 0 || itemType == ItemType.NONE)
                return 0;

            // remove item if slot is found
            if (inventoryItemDataCache.TryGetValue(itemType, out var inventoryItem))
                return RemoveItem(inventoryItem, amount);

            return 0;
        }

        /// <summary>
        /// Decrease item amount in 'inventorySlot' by 'amount'
        /// </summary>
        /// <param name="inventorySlot">inventory slot in which you want to decrease item amount</param>
        /// <param name="amount">amount to remove</param>
        /// <returns>removed items count</returns>
        public int RemoveItem(InventorySlot inventorySlot, int amount)
        {
            if (inventorySlot.IsNullOrEmpty())
                return 0;

            return RemoveItem(inventorySlot.Item, amount, inventorySlot);
        }

        /// <summary>
        /// Remove item from inventory and drop on the ground.
        /// </summary>
        /// <param name="inventorySlot">inventory slot from which you want to take item to drop</param>
        /// <param name="position">possition of dropped item</param>
        /// <param name="amount">amount to be dropped</param>
        /// <param name="velocityMultipler">velocity multipler (camera.forward * velocity)</param>
        /// <param name="rotate">should object be rotated same as camera</param>
        public void DropItem(InventorySlot inventorySlot, Vector3 position, int amount, float velocityMultipler, bool rotate = false)
        {
            if (inventorySlot.IsNullOrEmpty())
                return;

            RemoveItem(inventorySlot, amount);

            if (inventorySlot.Item.IsMaterial())
                DroppedItemsManager.Instance.DropItem(((InventoryItemMaterial)inventorySlot.Item).blockType, position, amount, velocityMultipler, rotate);
            else
                DroppedItemsManager.Instance.DropItem(inventorySlot.Item.Type, position, amount, velocityMultipler, PlayerController.ObjectInHand);
        }

        /// <summary>
        /// Set inventory slot as main hand slot
        /// </summary>
        public void SetInHandSlot(InventorySlot inventorySlot)
        {
            var temp = HandSlot;
            HandSlot = inventorySlot;
            OnMainHandUpdate?.Invoke(temp, inventorySlot);
        }
    }
}
