/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelTG.Player.Inventory;

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
        [Tooltip("Text in which item's icon will be displayed")]
        [SerializeField] private Image itemIconImage;

        private InventorySlot inventorySlot;

        /// <summary>
        /// Set item that will be displayed in this slot
        /// </summary>
        public void SetItem(InventorySlot inventorySlot)
        {
            if(inventorySlot == null || inventorySlot.Item == null)
            {
                OnSlotRemoved();
                return;
            }

            this.inventorySlot = inventorySlot;

            // set values in UI elements
            itemNameText.text = inventorySlot.ItemName;
            itemAmountText.text = inventorySlot.ItemAmount.ToString();
            itemWeightText.text = inventorySlot.ItemWeight.ToString();
            itemIconImage.sprite = inventorySlot.ItemIcon;

            // listen to events
            inventorySlot.OnAmountUpdate += OnAmountUpdate;
            inventorySlot.OnSlotRemoved += OnSlotRemoved;
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
            if(inventorySlot != null)
            {
                inventorySlot.OnAmountUpdate -= OnAmountUpdate;
                inventorySlot.OnSlotRemoved -= OnSlotRemoved;
            }
        }
    }
}
