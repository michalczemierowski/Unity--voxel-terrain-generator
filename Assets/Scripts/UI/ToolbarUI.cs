using UnityEngine;
using TMPro;
using UnityEngine.UI;
using VoxelTG.Player.Inventory;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    public class ToolbarUI : MonoBehaviour
    {
        /// <summary>
        /// Toolbar background images
        /// </summary>
        [SerializeField] private Image[] toolbarBackgroundImage;
        /// <summary>
        /// Toolbar item count texts
        /// </summary>
        [SerializeField] private TMP_Text[] toolbarSlotCount;
        /// <summary>
        /// Toolbar item icon images
        /// </summary>
        [SerializeField] private Image[] toolbarSlotIcon;

        /// <summary>
        /// How many toolbar slots are available
        /// </summary>
        /// <returns></returns>
        public int GetToolbarSlotCount()
        {
            return toolbarBackgroundImage.Length;
        }

        /// <summary>
        /// Change toolbar slot data
        /// </summary>
        /// <param name="slot">toolbar slot index</param>
        /// <param name="icon">inventory slot data</param>
        public void SetToolbarData(int slot, InventoryItemData inventorySlotData)
        {
            toolbarSlotIcon[slot].sprite = inventorySlotData.ItemIcon;

            // set color to alpha if sprite is null
            toolbarSlotIcon[slot].color = inventorySlotData.ItemIcon == null ? new Color(0, 0, 0, 0) : Color.white;

            toolbarSlotCount[slot].text = inventorySlotData.itemCount.ToString();
        }

        /// <summary>
        /// Remove item from toolbar slot
        /// </summary>
        /// <param name="slot">toolbar slot index</param>
        public void ClearToolbarSlot(int slot)
        {
            toolbarSlotIcon[slot].color = new Color(0, 0, 0, 0);
            toolbarSlotCount[slot].text = string.Empty;
        }

        /// <summary>
        /// Change toolbar slot bacground color
        /// </summary>
        /// <param name="slot">toolbar slot index (starting from 1)</param>
        /// <param name="color">background color</param>
        public void SetToolbarBackgroundColor(int slot, Color color)
        {
            toolbarBackgroundImage[slot].color = color;
        }
    }
}
