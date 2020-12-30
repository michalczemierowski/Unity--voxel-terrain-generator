using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelTG.Extensions;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    [RequireComponent(typeof(Button))]
    public class InventoryLinkedSlotUI : MonoBehaviour
    {
        [Tooltip("Text in which item's name will be displayed")]
        [SerializeField] private TMP_Text itemNameText;
        [Tooltip("Text in which item's amount will be displayed")]
        [SerializeField] private TMP_Text itemAmountText;
        [SerializeField] private Image itemIconImage;

        private Button m_Button;
        private Sprite defaultIcon;

        private InventorySlot linkedSlot;
        /// <summary>
        /// Reference to linked slot, may be null
        /// </summary>
        public InventorySlot LinkedSlot
        {
            get => linkedSlot;
            set
            {
                // remove listener from previous slot
                if (linkedSlot != null)
                {
                    linkedSlot.OnAmountUpdate -= OnAmountUpdate;
                    linkedSlot.OnSlotRemoved -= OnSlotRemoved;
                }

                linkedSlot = value;
                if (!value.IsNullOrEmpty())
                {
                    itemNameText.text = value.ItemName;
                    itemAmountText.text = value.ItemAmount.ToString();
                    itemIconImage.sprite = value.ItemIcon;

                    // add listeners to new slot
                    value.OnAmountUpdate += OnAmountUpdate;
                    value.OnSlotRemoved += OnSlotRemoved;
                }
                else
                {
                    itemNameText.text = string.Empty;
                    itemAmountText.text = string.Empty;
                    itemIconImage.sprite = defaultIcon;
                }
            }
        }
        public int Index { get; set; }
        private void OnAmountUpdate(int newAmount, int newWeight)
        {
            itemAmountText.text = newAmount.ToString();
        }

        private void OnSlotRemoved()
        {
            LinkedSlot = null;
        }

        private void Awake()
        {
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(OnClick);
            defaultIcon = itemIconImage.sprite;
        }

        private void OnClick()
        {
            if (linkedSlot == null)
                UIManager.InventoryUI.TryToLinkSlot(this, PlayerController.InventorySystem.HandSlot);
            // if there's already linked item, remove link
            else
                UIManager.InventoryUI.TryToLinkSlot(this, null);
        }
    }
}