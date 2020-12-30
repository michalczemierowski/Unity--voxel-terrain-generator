using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    public class InventorySlotUI : MonoBehaviour
    {
        [Tooltip("Text in which item's name will be displayed")]
        [SerializeField] private TMP_Text itemNameText;
        [Tooltip("Text in which item's amount will be displayed")]
        [SerializeField] private TMP_Text itemAmountText;
        [Tooltip("Text in which item's total weight will be displayed")]
        [SerializeField] private TMP_Text itemWeightText;
        [Tooltip("Image in which item's icon will be displayed")]
        [SerializeField] private Image itemIconImage;
        [SerializeField] private Button selectItemButton;
        [Tooltip("GameObject that will be enabled when item is in main hand")]
        [SerializeField] private GameObject activeOverlay;

        private InventorySlot linkedSlot;
        /// <summary>
        /// InventorySlot to which this slot is linked
        /// </summary>
        public InventorySlot LinkedSlot => linkedSlot;

        /// <summary>
        /// Set item that will be displayed in this slot
        /// </summary>
        public void SetItem(InventorySlot inventorySlot)
        {
            if (inventorySlot == null || inventorySlot.Item == null)
            {
                OnSlotRemoved();
                return;
            }

            this.linkedSlot = inventorySlot;

            // set values in UI elements
            itemNameText.text = inventorySlot.ItemName;
            itemAmountText.text = inventorySlot.ItemAmount.ToString();
            itemWeightText.text = inventorySlot.ItemWeight.ToString();
            itemIconImage.sprite = inventorySlot.ItemIcon;

            // set on click listener
            selectItemButton.onClick.AddListener(() =>
            {
                PlayerController.InventorySystem.SetInHandSlot(inventorySlot);
            });

            // listen to events
            inventorySlot.OnAmountUpdate += OnAmountUpdate;
            inventorySlot.OnSlotRemoved += OnSlotRemoved;
        }

        public void SetOverlayActive(bool active)
        {
            // to prevent null exception when removing slot
            if(activeOverlay != null)
                activeOverlay.SetActive(active);
        }

        private void OnAmountUpdate(int newAmount, int newWeight)
        {
            itemAmountText.text = newAmount.ToString();
            itemWeightText.text = newWeight.ToString();
        }

        private void OnSlotRemoved()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // when UI gets destroyed but inventory slot still exists (e.g. when updating UI)
            if (linkedSlot != null)
            {
                linkedSlot.OnAmountUpdate -= OnAmountUpdate;
                linkedSlot.OnSlotRemoved -= OnSlotRemoved;
            }
        }
    }
}
