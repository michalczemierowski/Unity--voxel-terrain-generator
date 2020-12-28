using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VoxelTG.Player.Inventory;

namespace VoxelTG.UI
{
    [System.Serializable] public class InventoryGroupToggleEvent : UnityEvent<ItemGroup, bool> { }

    [RequireComponent(typeof(Button))]
    public class InventoryGroupToggle : MonoBehaviour
    {
        [SerializeField] private ItemGroup targetGroup;
        public ItemGroup TargetGroup => targetGroup;
        public InventoryGroupToggleEvent OnInventoryGroupToggle;

        private Button button;
        public bool IsToggled { get; set; }

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                IsToggled = !IsToggled;
                OnInventoryGroupToggle?.Invoke(targetGroup, IsToggled);
            });
        }
    }
}