using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
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
        public int OccupiedCarringCapacity { get; private set; }

        public bool IsOverloaded => OccupiedCarringCapacity > carryingCapacity;

        /// <summary>
        /// Array containing all inventory slots
        /// </summary>
        public HashSet<InventorySlot> InventorySlots { get; private set; }

        private Dictionary<ItemType, InventoryItemBase> inventoryItemDataCache;
        private Dictionary<BlockType, InventoryItemBase> inventoryMaterialDataCache;

        #endregion

        #region // === Events === \\

        public delegate void MainHandUpdate(InventorySlot oldItem, InventorySlot newItem);
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

        private void OnInventoryContentsChangeListener(HashSet<InventorySlot> inventorySlots, bool onlyAmountChanged)
        {
            OccupiedCarringCapacity = 0;
            foreach (var slot in inventorySlots)
            {
                OccupiedCarringCapacity += slot.ItemWeight * slot.ItemAmount;
            }
        }

        private void Awake()
        {
            InventorySlots = new HashSet<InventorySlot>();
            inventoryItemDataCache = new Dictionary<ItemType, InventoryItemBase>();
            inventoryMaterialDataCache = new Dictionary<BlockType, InventoryItemBase>();

            HandSlot = new InventorySlot(null, 0);

            OnInventoryContentsChange += OnInventoryContentsChangeListener;
        }

        private void AddItem(InventoryItemBase inventoryItem, int amount)
        {
            if (amount < 1 || inventoryItem == null)
                return;

            InventorySlot existingSlot = InventorySlots.FirstOrDefault((item) => item.ItemName.Equals(inventoryItem.ItemName));
            if (existingSlot != null)
            {
                existingSlot.ItemAmount += amount;
                OnInventoryContentsChange?.Invoke(InventorySlots, true);
            }
            else
            {
                InventorySlot newSlot = new InventorySlot(inventoryItem, amount);
                InventorySlots.Add(newSlot);
                OnInventoryContentsChange?.Invoke(InventorySlots, false);
            }
        }

        private void RemoveItem(InventoryItemBase inventoryItem, int amount = 1, InventorySlot existingSlot = null)
        {
            if (existingSlot == null)
                existingSlot = InventorySlots.FirstOrDefault((item) => item.ItemName.Equals(inventoryItem.ItemName));

            if (existingSlot != null)
            {
                // if toRemove < 1 - remove item from inventory
                if (amount < 1)
                {
                    InventorySlots.Remove(existingSlot);
                    OnInventoryContentsChange?.Invoke(InventorySlots, false);
                }
                // else substract toRemove from count
                else
                {
                    int amountLeft = existingSlot.ItemAmount - amount;
                    if (amountLeft < 1)
                    {
                        InventorySlots.Remove(existingSlot);
                        OnInventoryContentsChange?.Invoke(InventorySlots, false);
                    }
                    else
                    {
                        existingSlot.ItemAmount = amountLeft;
                        OnInventoryContentsChange?.Invoke(InventorySlots, true);
                    }
                }
            }
        }

        public void AddItem(ItemType itemType, int amount)
        {
            if (amount < 1 || itemType == ItemType.NONE)
                return;

            // item already in cache
            if (inventoryItemDataCache.TryGetValue(itemType, out var item))
            {
                AddItem(item, amount);
            }
            else
            {
                string path = PATH_TO_ITEMS_DATA + itemType.ToString() + ".asset";
                Addressables.LoadAssetAsync<InventoryItemBase>(path).Completed += (handle) =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"Failed to load item data for {itemType} ({path})");
                        return;
                    }

                    inventoryItemDataCache.Add(itemType, handle.Result);
                    AddItem(handle.Result, amount);
                };
            }
        }

        public void AddItem(BlockType blockType, int amount)
        {
            if (amount < 1 || blockType == BlockType.AIR)
                return;

            // item already in cache
            if (inventoryMaterialDataCache.TryGetValue(blockType, out var item))
            {
                AddItem(item, amount);
            }
            else
            {
                string path = PATH_TO_MATERIAL_DATA + blockType.ToString() + ".asset";
                Addressables.LoadAssetAsync<InventoryItemBase>(path).Completed += (handle) =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError($"Failed to load item data for {blockType} ({path})");
                        return;
                    }

                    inventoryMaterialDataCache.Add(blockType, handle.Result);
                    AddItem(handle.Result, amount);
                };
            }
        }

        public void RemoveItem(ItemType itemType, BlockType blockType = BlockType.AIR, int amount = 1)
        {
            if (amount == 0)
                return;

            if (itemType == ItemType.MATERIAL && blockType != BlockType.AIR)
            {
                if (inventoryMaterialDataCache.TryGetValue(blockType, out var inventoryItem))
                    RemoveItem(inventoryItem, amount);
            }
            else if (itemType != ItemType.NONE)
            {
                if (inventoryItemDataCache.TryGetValue(itemType, out var inventoryItem))
                    RemoveItem(inventoryItem, amount);
            }
        }

        public void RemoveItem(InventorySlot inventorySlot, int amount)
        {
            if (inventorySlot == null || inventorySlot.IsEmpty())
                return;

            int toRemove = Mathf.Min(inventorySlot.ItemAmount, amount);
            RemoveItem(inventorySlot.Item, toRemove, inventorySlot);
        }

        public void DropItem(InventorySlot inventorySlot, Vector3 position, int count, float velocityMultipler)
        {
            // TODO
        }

        public void SelectInventorySlot(InventorySlot inventorySlot)
        {
            if (inventorySlot == null)
                return;

            OnMainHandUpdate?.Invoke(HandSlot, inventorySlot);
            HandSlot = inventorySlot;
        }
    }
}
