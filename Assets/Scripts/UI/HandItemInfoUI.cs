using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelTG.Extensions;
using VoxelTG.Player.Inventory;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    public class HandItemInfoUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string valueNotFoundString = "-";

        [Header("References")]
        [SerializeField] private GameObject toolItemGroupUI;
        [SerializeField] private GameObject weaponItemGroupUI;

        [Header("Base item")]
        [Tooltip("Text in which item's name will be displayed")]
        [SerializeField] private TMP_Text itemNameText;
        [Tooltip("Text in which item's amount will be displayed")]
        [SerializeField] private TMP_Text itemAmountText;
        [Tooltip("Image in which item's icon will be displayed")]
        [SerializeField] private Image itemIconImage;

        [Header("Tool")]
        [SerializeField] private TMP_Text toolCurrentDurabilityText;
        [SerializeField] private TMP_Text toolMaxDurabilityText;

        [Header("Weapon")]
        [SerializeField] private TMP_Text weaponCurrentClipText;
        [SerializeField] private TMP_Text weaponMaxClipText;
        [SerializeField] private TMP_Text weaponAmmoText;

        private InventorySlot activeSlot;

        public void SetUIActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetSlot(InventorySlot inventorySlot)
        {
            // return if slot is already displayed
            if (inventorySlot == activeSlot)
                return;

            // remove event listeners from previous slot, if slot is not null
            if (activeSlot != null)
            {
                activeSlot.OnMetadataUpdate -= OnMetadataUpdate;
                activeSlot.OnAmountUpdate -= OnAmountUpdate;
                if (activeSlot.Item.IsWeapon)
                {
                    var weapon = (InventoryItemWeapon)activeSlot.Item;
                    // remove listener from previous weapon ammo slot
                    if (Player.PlayerController.InventorySystem.TryFindInventorySlotWithItem(weapon.AmmoItemType, out var ammoSlot))
                    {
                        ammoSlot.OnAmountUpdate -= OnWeaponAmmoAmountUpdate;
                    }
                }
            }

            activeSlot = inventorySlot;

            // if new slot is null or empty
            if (activeSlot.IsNullOrEmpty())
            {
                SetUIActive(false);
                return;
            }

            activeSlot.OnMetadataUpdate += OnMetadataUpdate;
            activeSlot.OnAmountUpdate += OnAmountUpdate;

            var item = inventorySlot.Item;

            // show values that are displayed for any type of item
            itemNameText.text = item.Name;
            itemAmountText.text = inventorySlot.ItemAmount.ToString();
            itemIconImage.sprite = item.Icon;

            // enable/disable tool/weapon groups 
            toolItemGroupUI.SetActive(item.IsTool);
            weaponItemGroupUI.SetActive(item.IsWeapon);

            if (item.IsTool)
            {
                var tool = (InventoryItemTool)item;

                // try to get durability metadata
                toolCurrentDurabilityText.text = inventorySlot.TryGetMedadata<int>(InventoryItemTool.DURABILITY_MDK, out MetadataProperty<int> durabilityMeta)
                    ? durabilityMeta.ValueStr
                    : valueNotFoundString;

                toolMaxDurabilityText.text = tool.Durability.ToString();
            }
            else if (item.IsWeapon)
            {
                var weapon = (InventoryItemWeapon)item;

                // try to get clip metadata
                weaponCurrentClipText.text = inventorySlot.TryGetMedadata<int>(InventoryItemWeapon.AMMO_CURRENT_CLIP_MDK, out MetadataProperty<int> clipMeta)
                    ? clipMeta.ValueStr
                    : valueNotFoundString;

                weaponMaxClipText.text = weapon.ClipSize.ToString();

                // try to find amount of ammo in inventory
                if (Player.PlayerController.InventorySystem.TryFindInventorySlotWithItem(weapon.AmmoItemType, out var ammoSlot))
                {
                    weaponAmmoText.text = ammoSlot.ItemAmount.ToString();

                    // register event listener for ammo amount update
                    ammoSlot.OnAmountUpdate += OnWeaponAmmoAmountUpdate;
                }
                else
                    weaponAmmoText.text = valueNotFoundString;
            }

            // enable UI
            SetUIActive(true);
        }

        private void OnAmountUpdate(int newAmount, int newWeight)
        {
            itemAmountText.text = newAmount.ToString();
        }

        private void OnWeaponAmmoAmountUpdate(int newAmount, int newWeight)
        {
            weaponAmmoText.text = newAmount.ToString();
        }

        private void OnMetadataUpdate(string key, IMetadataProperty newData)
        {
            if (activeSlot.IsNullOrEmpty())
                return;

            var item = activeSlot.Item;
            if (item.IsTool)
            {
                var tool = (InventoryItemTool)item;

                // when current durability changes
                if (key.Equals(InventoryItemTool.DURABILITY_MDK))
                {
                    // try to read durability from metadata
                    toolCurrentDurabilityText.text = newData is MetadataProperty<int> durabilityMeta
                        ? durabilityMeta.ValueStr
                        : valueNotFoundString;
                }
            }
            else if (item.IsWeapon)
            {
                var weapon = (InventoryItemWeapon)item;

                // when clipp ammo count changes
                if (key.Equals(InventoryItemWeapon.AMMO_CURRENT_CLIP_MDK))
                {
                    weaponCurrentClipText.text = newData is MetadataProperty<int> clipMeta
                        ? clipMeta.ValueStr
                        : valueNotFoundString;
                }
            }
        }
    }
}
