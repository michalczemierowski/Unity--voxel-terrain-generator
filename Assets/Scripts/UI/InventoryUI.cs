using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    /// <summary>
    /// Class capable of handling Inventory UI
    /// </summary>
    public class InventoryUI : MonoBehaviour, IToggleableUI
    {
        [Tooltip("Text in which name of item in hand will be displayed")]
        [SerializeField] private TMP_Text currentItemName;

        [Tooltip("Parent for all inventory slots")]
        [SerializeField] private RectTransform inventorySlotsParent;

        [Tooltip("Inventory item prefab, must contain InventorySlotUI component")]
        [SerializeField] private GameObject inventoryItemUIPrefab;

        [Header("Capacity")]

        [Tooltip("Text in which occupied and max carrying capacity will be displayed")]
        [SerializeField] private TMP_Text carryingCapacityText;

        [Tooltip("Color of Carrying Capacity Text when player is overweighted")]
        [SerializeField] private Color capacityColorOverloaded;

        [Header("Linked slots")]

        [Tooltip("Array that should containg all linked slots")]
        [SerializeField] private InventoryLinkedSlotUI[] linkedSlotUIs;

        /// <summary>
        /// list of UI slots (used to enable/disable slots by groups
        /// </summary>
        private List<InventorySlotUI> inventoryUISlots;
        /// <summary>
        /// Array containing all group toggles (used to make sure only one is toggled)
        /// </summary>
        private InventoryGroupToggle[] groupToggles;

        private Color capacityColorDefault;

        /// <summary>
        /// Should slots be updated when opening inventory UI next time (inventory was updated when UI was closed)
        /// </summary>
        private bool shouldUpdateUI;

        /// <summary>
        /// True if inventory UI is opened
        /// </summary>
        public bool IsInventoryOpened => gameObject.activeSelf;

        /// <summary>
        /// How many linked slots are available
        /// </summary>
        public int LinkedSlotsCount => linkedSlotUIs.Length;

        public void Init()
        {
            PlayerController.InventorySystem.OnInventoryContentsChange += OnInventoryContentsChange;
            PlayerController.InventorySystem.OnMainHandUpdate += OnMainHandUpdate;

            groupToggles = GetComponentsInChildren<InventoryGroupToggle>();

            capacityColorDefault = carryingCapacityText.color;

            for (int i = 0; i < linkedSlotUIs.Length; i++)
            {
                linkedSlotUIs[i].Index = i;
            }
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
            {
                currentItemName.text = string.Empty;

                // no overlay should be active when hand is empty
                foreach (var slotUI in inventoryUISlots)
                    slotUI.SetOverlayActive(false);
            }
            else
            {
                // TODO: display more info
                currentItemName.text = newItem.ItemName;

                // enable overlay on used item
                foreach (var slotUI in inventoryUISlots)
                {
                    bool active = slotUI.LinkedSlot == newItem;
                    slotUI.SetOverlayActive(active);
                }
            }
        }

        private void OnInventoryContentsChange(HashSet<InventorySlot> inventorySlots, bool onlyAmountChanged)
        {
            carryingCapacityText.text = PlayerController.InventorySystem.OccupiedCarringCapacity + "/" + PlayerController.InventorySystem.CarryingCapacity;
            carryingCapacityText.color = PlayerController.InventorySystem.IsOverloaded ? capacityColorOverloaded : capacityColorDefault;

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
        public void ToggleUI()
        {
            bool active = !IsInventoryOpened;
            gameObject.SetActive(active);

            UIManager.ToggleUIMode(active, this);

            // if inventory content has changed when UI was disabled
            if (active && shouldUpdateUI)
            {
                UpdateInventoryUI();
                shouldUpdateUI = false;
            }
        }

        /// <summary>
        /// Get linked slot by index
        /// </summary>
        /// <param name="index">index that should be in range from 0 to <see cref="LinkedSlotsCount"/> - 1</param>
        /// <returns>linked inventory slot (may be null or contain null item)</returns>
        public InventoryLinkedSlotUI GetLinkedSlot(int index)
        {
            if (index < 0 || index >= linkedSlotUIs.Length)
            {
                Debug.LogError($"Index out of range. index: {index} range:<0; {linkedSlotUIs.Length - 1}>", this);
                return null;
            }

            InventoryLinkedSlotUI linkedSlotUI = linkedSlotUIs[index];
            return linkedSlotUI;
        }

        /// <summary>
        /// Try to link InventorySlot to InventoryLinkedSlotUI (each InventorySlot may be linked to only one InventoryLinkedSlotUI)
        /// </summary>
        public void TryToLinkSlot(InventoryLinkedSlotUI linkedSlotUI, InventorySlot inventorySlot)
        {
            if (linkedSlotUI == null)
                return;

            // check only for non null slots
            if (inventorySlot != null)
            {
                // check if inventory slot is linked to any other InventoryLinkedSlotUI
                // and return if so
                foreach (var linkedSlot in linkedSlotUIs)
                {
                    if (linkedSlot.LinkedSlot == inventorySlot)
                        return;
                }
            }

            // else, link slot
            linkedSlotUI.LinkedSlot = inventorySlot;
        }
    }
}
