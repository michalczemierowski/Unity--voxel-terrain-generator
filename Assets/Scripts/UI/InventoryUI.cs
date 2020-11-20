using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;
using VoxelTG.DebugUtils;
using VoxelTG.Entities.Items;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    // TODO: xml docs
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private ToolbarUI toolbarUI;
        [SerializeField] private TMP_Text currentItemName;

        [Header("Settings")]
        [SerializeField] private bool debugToolbar;
        [SerializeField] private Color defaultToolbarBgColor;
        [SerializeField] private Color selectedToolbarBgColor;

        private InventoryItemData[] toolbarInventoryData;
        private InventoryItemData[] inventoryData;

        private int toolbarSlotsCount;

        private int selectedToolbarSlot;
        public InventoryItemData ActiveToolbarSlot => toolbarInventoryData[selectedToolbarSlot];

        public delegate void ToolbarUpdate(int slot, InventoryItemData oldItem, InventoryItemData newItem);
        public ToolbarUpdate OnToolbarUpdate;

        public delegate void ToolbarSlotSelected(int slot);
        public ToolbarSlotSelected OnToolbarSlotSelected;

        public delegate void ActiveToolbarSlotUpdate(InventoryItemData oldItem, InventoryItemData newItem);
        public ActiveToolbarSlotUpdate OnActiveToolbarSlotUpdate;

        private InventoryItemBase[] toolData;
        private InventoryItemMaterial[] materialData;

        private void Awake()
        {
            toolbarSlotsCount = toolbarUI.GetToolbarSlotCount();
            toolbarInventoryData = new InventoryItemData[toolbarSlotsCount];

            // init positions for toolbar
            for (int i = 0; i < toolbarInventoryData.Length; i++)
            {
                toolbarInventoryData[i].positionX = i;
            }

            SelectToolbarSlot(0);


            // load items from resources and store them in arrays
            InventoryItemBase[] inventoryItems = Resources.LoadAll<InventoryItemBase>("Scriptable Objects/Items/");
            toolData = new InventoryItemBase[Enum.GetValues(typeof(ItemType)).Length];
            materialData = new InventoryItemMaterial[Enum.GetValues(typeof(BlockType)).Length];
            foreach (InventoryItemBase item in inventoryItems)
            {
                if (item is InventoryItemMaterial itemMaterial)
                {
                    materialData[(int)itemMaterial.blockType] = itemMaterial;
                }
                else if (item is InventoryItemTool itemTool)
                {
                    toolData[(int)itemTool.itemType] = itemTool;
                }
                else if (item is InventoryItemWeapon itemWeapon)
                {
                    toolData[(int)itemWeapon.itemType] = itemWeapon;
                }
            }

            // debug
            if (debugToolbar)
            {
                OnToolbarUpdate += DebugOnToolbarUpdate;
            }

            OnActiveToolbarSlotUpdate += OnActiveToolbarSlotUpdateListener;
        }

        #region  // === DEBUG == \\

        private void DebugOnToolbarUpdate(int slot, InventoryItemData oldItem, InventoryItemData newItem)
        {
            DebugConsole.AddDebugMessageStatic($"UPDATE TOOLBAR SLOT [{slot}] {newItem.ItemType} {newItem.BlockType} {newItem.itemCount}");
        }

        #endregion

        #region  // === Toolbar === \\

        private void OnActiveToolbarSlotUpdateListener(InventoryItemData oldItem, InventoryItemData newItem)
        {
            currentItemName.text = newItem.ItemName;
        }

        /// <summary>
        /// Check if slot is out of toolbar range
        /// </summary>
        /// <param name="slot">index of toolbar slot</param>
        /// <returns>true if out of range</returns>
        public bool OutOfToolbarRange(int slot)
        {
            if (slot < 0 || slot >= toolbarSlotsCount)
                return true;
            return false;
        }

        /// <summary>
        /// Remove item from inventory and instantiate item pickup
        /// </summary>
        /// <param name="position">item pickup spawn position</param>
        /// <param name="count">item count</param>
        /// <param name="velocity">item pickup velocity multipler (transform.forward * velocity)</param>
        public void DropToolbarItem(Vector3 position, int count = 1, float velocity = 2f)
        {
            if (ActiveToolbarSlot.IsEmpty())
                return;

            InventoryItemData data = ActiveToolbarSlot;

            UpdateToolbarItemCount(ActiveToolbarSlot.positionX, -count);

            if (data.IsMaterial())
                DroppedItemsManager.Instance.DropItemMaterial(data.BlockType, position, count, velocity);
            else if (data.IsTool())
                DroppedItemsManager.Instance.DropItemTool(data.ItemType, position, count, velocity);
            else if (data.IsWeapon())
            // TODO: add weapon
            {
                DroppedItemsManager.Instance.DropItemTool(data.ItemType, position, count, velocity, PlayerController.WeaponController.currentWeaponGameObject);
            }
        }

        /// <summary>
        /// Set active toolbar slot
        /// </summary>
        /// <param name="slot">index of toolbar slot</param>
        public void SelectToolbarSlot(int slot)
        {
            if (OutOfToolbarRange(slot))
                return;

            toolbarUI.SetToolbarBackgroundColor(ActiveToolbarSlot.positionX, defaultToolbarBgColor);
            toolbarUI.SetToolbarBackgroundColor(slot, selectedToolbarBgColor);

            InventoryItemData oldItem = ActiveToolbarSlot;

            selectedToolbarSlot = slot;

            OnActiveToolbarSlotUpdate?.Invoke(oldItem, ActiveToolbarSlot);
            OnToolbarSlotSelected?.Invoke(slot);
        }

        /// <summary>
        /// Set toolbar item
        /// </summary>
        /// <param name="slot">index of toolbar slot</param>
        /// <param name="inventorySlotData">item data</param>
        public void SetToolbarItem(int slot, InventoryItemData inventorySlotData)
        {
            if (OutOfToolbarRange(slot))
                return;

            InventoryItemData oldItem = toolbarInventoryData[slot];

            if (inventorySlotData.IsEmpty() || inventorySlotData.itemCount <= 0)
            {
                ClearToolbarSlot(slot);
            }
            else
            {
                toolbarInventoryData[slot] = inventorySlotData;
                toolbarUI.SetToolbarData(slot, inventorySlotData);

                toolbarInventoryData[slot] = inventorySlotData;
            }

            if (slot == selectedToolbarSlot)
                OnActiveToolbarSlotUpdate?.Invoke(oldItem, ActiveToolbarSlot);

            OnToolbarUpdate?.Invoke(slot, oldItem, toolbarInventoryData[slot]);
        }

        /// <summary>
        /// Set toolbar item to empty
        /// </summary>
        /// <param name="slot">index of toolbar slot</param>
        public void ClearToolbarSlot(int slot)
        {
            toolbarInventoryData[slot] = InventoryItemData.EMPTY;
            toolbarInventoryData[slot].positionX = slot;

            toolbarUI.ClearToolbarSlot(slot);
        }

        /// <summary>
        /// Update toolbar item count
        /// </summary>
        /// <param name="slot">index of toolbar slot</param>
        /// <param name="change">value that will be added to current count (use negative values if you want to decrease count)</param>
        public void UpdateToolbarItemCount(int slot, int change)
        {
            InventoryItemData inventorySlotData = toolbarInventoryData[slot];
            inventorySlotData.itemCount += change;

            SetToolbarItem(slot, inventorySlotData);
        }

        /// <summary>
        /// Get item from toolbar slot
        /// </summary>
        /// <param name="slot">index of toolbar slot</param>
        public InventoryItemData GetToolbarItem(int slot)
        {
            if (OutOfToolbarRange(slot))
                return new InventoryItemData();

            return toolbarInventoryData[slot];
        }

        #endregion

        #region  // === Inventory === \\

        /// <summary>
        /// Add item to inventory if there is free space
        /// </summary>
        /// <param name="itemToAdd">item data</param>
        /// <returns>true if success</returns>
        public bool AddItemToInventory(InventoryItemData itemToAdd)
        {
            if (itemToAdd.IsEmpty())
                return false;

            // search for same item
            for (int i = 0; i < toolbarSlotsCount; i++)
            {
                InventoryItemData toolbarData = toolbarInventoryData[i];
                if (toolbarData.IsSameItem(itemToAdd))
                {
                    UpdateToolbarItemCount(i, itemToAdd.itemCount);
                    return true;
                }
            }

            // search for empty slot
            for (int i = 0; i < toolbarSlotsCount; i++)
            {
                InventoryItemData toolbarData = toolbarInventoryData[i];
                if (toolbarData.IsEmpty())
                {
                    itemToAdd.positionX = i;
                    SetToolbarItem(i, itemToAdd);
                    return true;
                }
            }

            return false;
        }

        #endregion

        public InventoryItemBase GetToolItemData(ItemType toolType)
        {
            //itemType = toolData[(int)toolType].GetType();
            return toolData[(int)toolType];
        }

        public InventoryItemMaterial GetMaterialItemData(BlockType blockType)
        {
            return materialData[(int)blockType];
        }
    }
}
