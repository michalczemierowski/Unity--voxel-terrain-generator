using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VoxelTG.Player.Inventory;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    [System.Serializable] public class InventoryGroupToggleEvent : UnityEvent<ItemGroup, bool> { }

    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Image))]
    public class InventoryGroupToggle : MonoBehaviour
    {
        [SerializeField] private Color toggledColor;
        [SerializeField] private ItemGroup targetGroup;
        public ItemGroup TargetGroup => targetGroup;

        private Button button;
        private Image image;
        private Color defualtColor;

        /// <summary>
        /// Event that will be called when toggle switch changes state
        /// </summary>
        public InventoryGroupToggleEvent OnInventoryGroupToggle;

        private bool isToggled;
        /// <summary>
        /// Is filtering enabled
        /// </summary>
        public bool IsToggled
        {
            get => isToggled;
            set
            {
                image.color = value ? toggledColor : defualtColor;
                isToggled = value;
            }
        }

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                IsToggled = !IsToggled;
                OnInventoryGroupToggle?.Invoke(targetGroup, IsToggled);
            });

            image = GetComponent<Image>();
            defualtColor = image.color;
        }
    }
}