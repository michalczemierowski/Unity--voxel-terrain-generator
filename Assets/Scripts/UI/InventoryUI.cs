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

        [Tooltip("Text in which occupied and max carrying capacity will be displayed")]
        [SerializeField] private TMP_Text carringCapacityText;

        /// <summary>
        /// list of UI slots (used to enable/disable slots by groups
        /// </summary>
        private List<InventorySlotUI> inventoryUISlots;
        /// <summary>
        /// Array containing all group toggles (used to make sure only one is toggled)
        /// </summary>
        private InventoryGroupToggle[] groupToggles;

        /// <summary>
        /// Should slots be updated when opening inventory UI next time (inventory was updated when UI was closed)
        /// </summary>
        private bool shouldUpdateUI;
        /// <summary>
        /// True if inventory UI is opened
        /// </summary>
        public bool IsInventoryOpened => gameObject.activeSelf;

        public void Init()
        {
            PlayerController.InventorySystem.OnInventoryContentsChange += OnInventoryContentsChange;
            PlayerController.InventorySystem.OnMainHandUpdate += OnMainHandUpdate;

            groupToggles = GetComponentsInChildren<InventoryGroupToggle>();
        }

        private void OnDestroy()
        {
            PlayerController.InventorySystem.OnInventoryContentsChange -= OnInventoryContentsChange;
            PlayerController.InventorySystem.OnMainHandUpdate -= OnMainHandUpdate;
        }

        private void OnMainHandUpdate(InventorySlot oldItem, InventorySlot newItem)
        {
            // display name of item in hand
            if (newItem == null)
                currentItemName.text = string.Empty;
            else
                currentItemName.text = newItem.ItemName;
        }

        private void OnInventoryContentsChange(HashSet<InventorySlot> inventorySlots, bool onlyAmountChanged)
        {
            carringCapacityText.text = PlayerController.InventorySystem.OccupiedCarringCapacity + "/" + PlayerController.InventorySystem.CarryingCapacity;
            // amount updates are handled in InventorySlotUI
            if (onlyAmountChanged)
                return;
            // don't need to update elements when inventory is closed
            if (!IsInventoryOpened)
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
            inventoryUISlots = new List<InventorySlotUI>(inventorySlots.Count);
            foreach (var slot in inventorySlots)
            {
                if (slot == null)
                    continue;

                // TODO: pooling
                InventorySlotUI slotUI = Instantiate(inventoryItemUIPrefab, inventorySlotsParent).GetComponent<InventorySlotUI>();
                slotUI.SetItem(slot);

                inventoryUISlots.Add(slotUI);
            }


            // check if any toggle was enabled before updating slots
            foreach (var groupToggle in groupToggles)
            {
                if (groupToggle.IsToggled)
                {
                    // and toggle filters if they were enabled before
                    ToggleGroupFiltering(groupToggle.TargetGroup, true);
                }
            }
        }

        /// <summary>
        /// Toggle filtering items by group (show only items from specific group)
        /// </summary>
        /// <param name="itemGroup">group from which slots will be enabled</param>
        /// <param name="on">if true filter will be enabled, else all items will be visible</param>
        public void ToggleGroupFiltering(ItemGroup itemGroup, bool on)
        {
            if (inventoryUISlots == null)
                return;

            foreach (var groupToggle in groupToggles)
            {
                if (groupToggle.TargetGroup != itemGroup)
                    groupToggle.IsToggled = false;
            }

            foreach (var slotUI in inventoryUISlots)
            {
                if (slotUI == null)
                    continue;

                bool shouldBeActive = !on || slotUI.LinkedSlot.Item.Group == itemGroup;
                slotUI.gameObject.SetActive(shouldBeActive);
            }
        }

        /// <summary>
        /// Enable/disable inventory UI
        /// </summary>
        public void ToggleInventoryUI()
        {
            bool active = !IsInventoryOpened;
            gameObject.SetActive(active);

            UIManager.ToggleUIMode(active);

            // if inventory content has changed when UI was disabled
            if (active && shouldUpdateUI)
            {
                UpdateInventoryUI();
                shouldUpdateUI = false;
            }
        }
    }
}
