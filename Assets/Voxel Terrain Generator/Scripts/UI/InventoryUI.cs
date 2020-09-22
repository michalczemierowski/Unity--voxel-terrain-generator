/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using VoxelTG.Entities.Items;
using VoxelTG.Player.Inventory;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;

namespace VoxelTG.UI
{
    // TODO: add comments
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private ToolbarUI toolbarUI;
        [Header("Color settings")]
        [SerializeField] private Color defaultToolbarBgColor;
        [SerializeField] private Color selectedToolbarBgColor;

        private InventoryItemData[] toolbarInventoryData;
        private InventoryItemData[] inventoryData;

        private int toolbarSlotsCount;

        private int selectedToolbarSlot;
        public InventoryItemData ActiveToolbarSlot => toolbarInventoryData[selectedToolbarSlot];

        private InventoryItemTool[] toolData;
        private InventoryItemMaterial[] materialData;

        private void Awake()
        {
            toolbarSlotsCount = toolbarUI.GetToolbarCount();
            toolbarInventoryData = new InventoryItemData[toolbarSlotsCount];

            // init positions for toolbar
            for (int i = 0; i < toolbarInventoryData.Length; i++)
            {
                toolbarInventoryData[i].positionX = i;
            }

            SelectToolbarSlot(0);

            InventoryItem[] inventoryItems = Resources.LoadAll<InventoryItem>("Scriptable Objects/Items/");
            toolData = new InventoryItemTool[Enum.GetValues(typeof(ToolType)).Length];
            materialData = new InventoryItemMaterial[Enum.GetValues(typeof(BlockType)).Length];
            foreach (InventoryItem item in inventoryItems)
            {
                if (item is InventoryItemMaterial itemMaterial)
                {
                    materialData[(int)itemMaterial.blockType] = itemMaterial;
                }
                else if (item is InventoryItemTool itemTool)
                {
                    toolData[(int)itemTool.toolType] = itemTool;
                }
            }
        }

        #region  // === Toolbar === \\

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
                DroppedItemsManager.Instance.DropMaterial(data.BlockType, position, count, velocity);
            else if(data.IsTool())
                DroppedItemsManager.Instance.DropItem(data.ToolType, position, count, velocity);
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

            selectedToolbarSlot = slot;
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

            if (inventorySlotData.IsEmpty() || inventorySlotData.itemCount <= 0)
            {
                inventorySlotData = InventoryItemData.EMPTY;
                inventorySlotData.positionX = slot;
                toolbarUI.ClearToolbarSlot(slot);
            }
            else
            {
                toolbarInventoryData[slot] = inventorySlotData;
                toolbarUI.SetToolbarData(slot, inventorySlotData);
            }

            toolbarInventoryData[slot] = inventorySlotData;
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
            if (itemToAdd.Equals(InventoryItemData.EMPTY))
                return false;

            // search for same item
            for (int i = 0; i < toolbarSlotsCount; i++)
            {
                InventoryItemData toolbarData = toolbarInventoryData[i];
                if (toolbarData.ToolType == itemToAdd.ToolType && toolbarData.BlockType == itemToAdd.BlockType)
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

        public InventoryItemTool GetToolItemData(ToolType toolType)
        {
            return toolData[(int)toolType];
        }

        public InventoryItemMaterial GetMaterialItemData(BlockType blockType)
        {
            return materialData[(int)blockType];
        }

    }
}
