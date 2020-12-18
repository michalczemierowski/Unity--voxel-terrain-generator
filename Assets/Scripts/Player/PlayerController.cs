using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using VoxelTG.Player.Interactions;
using VoxelTG.Player.Inventory;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance;
        public static Transform PlayerTransform { get; private set; }
        public static bool AreControlsActive => EventSystem.current.currentSelectedGameObject == null;

        [Header("Settings")]
        [SerializeField] private float droppedItemVelocity = 3;

        [Header("References")]
        [SerializeField] private MouseLook m_MouseLook;
        public MouseLook MouseLook => m_MouseLook;

        [SerializeField] private Animator cameraAnimator;
        public Animator CameraAnimator => cameraAnimator;

        [SerializeField] private InventoryManager inventoryManager;
        public static InventoryManager InventoryManager => Instance.inventoryManager;

        [SerializeField] private PlayerMovement m_PlayerMovement;
        public static PlayerMovement Movement => Instance.m_PlayerMovement;

        [SerializeField] private PlayerTerrainInteractions m_TerrainInteractions;
        public static PlayerTerrainInteractions TerrainInteractions => Instance.m_TerrainInteractions;

        [SerializeField] private PlayerWeaponController m_WeaponController;
        public static PlayerWeaponController WeaponController => Instance.m_WeaponController;

        [SerializeField] private FlashlightController flashlightController;
        public static FlashlightController FlashlightController => Instance.flashlightController;

        [SerializeField] private Transform handTransform;

        public delegate void HandObjectLoaded(GameObject handObject, ItemType itemType);
        public HandObjectLoaded OnHandObjectLoaded;

        // TODO: move selection logic to InventoryUI
        private int currentlySelectedToolbarSlot;
        private InventoryUI inventoryUI;

        void Awake()
        {
            if (Instance)
                Destroy(this);
            else
            {
                Instance = this;
                PlayerTransform = transform;
            }

            gameObject.SetActive(false);
        }

        private void Start()
        {
            inventoryUI = UIManager.InventoryUI;
            inventoryUI.OnActiveToolbarSlotUpdate += OnActiveToolbarSlotUpdate;
        }

        private void OnDestroy()
        {
            if(inventoryUI != null)
                inventoryUI.OnActiveToolbarSlotUpdate -= OnActiveToolbarSlotUpdate;
        }

        private void Update()
        {
            if(AreControlsActive)
                HandleInput();
        }

        private void SelectToolbarSlot(int slot)
        {
            if (inventoryUI.OutOfToolbarRange(slot))
                return;

            inventoryUI.SelectToolbarSlot(slot);
            m_TerrainInteractions.ResetMining();

            currentlySelectedToolbarSlot = slot;
        }

        // TODO: input system
        private void HandleInput()
        {
            // toolbar slots 1 - 9
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown((KeyCode)(49 + i)))
                {
                    SelectToolbarSlot(i);
                }
            }

            // toolbar scrollwheel
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                SelectToolbarSlot(currentlySelectedToolbarSlot - 1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                SelectToolbarSlot(currentlySelectedToolbarSlot + 1);
            }

            // drop item
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!inventoryUI.SelectedToolbarSlot.IsEmpty())
                {
                    Transform cameraTransform = MouseLook.cameraTransform;
                    inventoryUI.DropToolbarItem(cameraTransform.position + cameraTransform.forward, 1, droppedItemVelocity);
                }
            }

            // flashlight
            if (Input.GetKeyDown(KeyCode.F))
                flashlightController.NextFlashLightMode();

            // TESTING
            if (Input.GetKeyDown(KeyCode.H))
            {
                // TODO: add items to inventory
            }

        }

        private void OnActiveToolbarSlotUpdate(InventorySlot oldItem, InventorySlot newItem)
        {
            if (!oldItem.IsSameType(newItem))
            {
                LoadInHandModel();
            }
        }

        private void OnHandObjectPrefabLoaded(AsyncOperationHandle<GameObject> obj, ItemType itemType)
        {
            if (inventoryUI.SelectedToolbarSlot.ItemType != itemType)
                return;

            GameObject prefab = obj.Result;
            if (prefab != null)
            {
                GameObject handObject = Instantiate(prefab, handTransform);
                OnHandObjectLoaded?.Invoke(handObject, itemType);
            }
        }

        private void LoadInHandModel()
        {
            for (int j = 0; j < handTransform.childCount; j++)
            {
                Destroy(handTransform.GetChild(j).gameObject);
            }

            ItemType itemType = inventoryUI.SelectedToolbarSlot.ItemType;
            if (inventoryUI.SelectedToolbarSlot.IsTool(out InventoryItemTool inventoryItemTool) && inventoryItemTool.addressablePathToModel != string.Empty)
            {
                Addressables.LoadAssetAsync<GameObject>(inventoryItemTool.addressablePathToModel).Completed += ((AsyncOperationHandle<GameObject> obj) => OnHandObjectPrefabLoaded(obj, itemType));
            }
            else if (inventoryUI.SelectedToolbarSlot.IsWeapon(out InventoryItemWeapon inventoryItemWeapon) && inventoryItemWeapon.addressablePathToModel != string.Empty)
            {
                Addressables.LoadAssetAsync<GameObject>(inventoryItemWeapon.addressablePathToModel).Completed += ((AsyncOperationHandle<GameObject> obj) => OnHandObjectPrefabLoaded(obj, itemType));
            }
        }
    }
}