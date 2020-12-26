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
    /// <summary>
    /// Class capable of handling Inventory UI
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Tooltip("Text in which name of item in hand will be displayed")]
        [SerializeField] private TMP_Text currentItemName;
        
        [Tooltip("Parent for all inventory slots")]
        [SerializeField] private RectTransform inventorySlotsParent;
        [Tooltip("Inventory item prefab, must contain InventorySlotUI component")]
        [SerializeField] private GameObject inventoryItemUIPrefab;

        private bool shouldUpdateUI;
        public bool IsInventoryOpened => gameObject.activeSelf;

        private void OnEnable()
        {
            PlayerController.InventorySystem.OnInventoryContentsChange += OnInventoryContentsChange;
            PlayerController.InventorySystem.OnMainHandUpdate += OnMainHandUpdate;
        }

        private void OnDestroy()
        {
            PlayerController.InventorySystem.OnInventoryContentsChange -= OnInventoryContentsChange;
            PlayerController.InventorySystem.OnMainHandUpdate -= OnMainHandUpdate;
        }

        private void OnMainHandUpdate(InventorySlot oldItem, InventorySlot newItem)
        {
            if (newItem == null)
                currentItemName.text = string.Empty;
            else
                currentItemName.text = newItem.ItemName;
        }

        private void OnInventoryContentsChange(HashSet<InventorySlot> inventorySlots, bool onlyAmountChanged)
        {
            // amount updates are handled in InventorySlotUI
            if (onlyAmountChanged)
                return;
            // don't need to update elements when inventory is closed
            if(!IsInventoryOpened)
            {
                shouldUpdateUI = true;
                return;
            }

            UpdateInventoryUI();
        }

        /// <summary>
        /// Clear and rebuild inventory UI
        /// </summary>
        private void UpdateInventoryUI()
        {
            ClearInventorySlotsParent();
            CreateInventoryItemsUI();
        }

        /// <summary>
        /// Remove all inventory UI slots
        /// </summary>
        private void ClearInventorySlotsParent()
        {
            // TODO: pooling
            for (int i = 0; i < inventorySlotsParent.childCount; i++)
            {
                Destroy(inventorySlotsParent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// Create UI objects for inventory slots
        /// </summary>
        private void CreateInventoryItemsUI()
        {
            HashSet<InventorySlot> inventorySlots = PlayerController.InventorySystem.InventorySlots;
            foreach(var slot in inventorySlots)
            {
                if (slot == null)
                    continue;

                // TODO: pooling
                InventorySlotUI slotUI = Instantiate(inventoryItemUIPrefab, inventorySlotsParent).GetComponent<InventorySlotUI>();
                slotUI.SetItem(slot);
            }
        }

        /// <summary>
        /// Enable/disable inventory UI
        /// </summary>
        public void ToggleInventoryUI()
        {
            bool active = !IsInventoryOpened;
            gameObject.SetActive(active);

            // if inventory content has changed when UI was disabled
            if(active && shouldUpdateUI)
            {
                UpdateInventoryUI();
                shouldUpdateUI = false;
            }
        }
    }
}
