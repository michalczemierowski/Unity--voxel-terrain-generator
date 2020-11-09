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
        public PlayerMovement m_PlayerMovement;
        public MouseLook m_MouseLook;
        public PlayerTerrainInteractions m_TerrainInteractions;
        public PlayerWeaponController m_WeaponController;
        public Animator cameraAnimator;

        [NonSerialized]
        public UIManager uiManager;

        [NonSerialized]
        public InventoryUI inventoryUI;

        [NonSerialized]
        private FlashLightController flashLightController;

        [SerializeField] private Transform handTransform;

        public delegate void HandObjectLoaded(GameObject handObject, ItemType itemType);
        public HandObjectLoaded OnHandObjectLoaded;

        private int currentlySelectedToolbarSlot;

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
            flashLightController = GetComponentInChildren<FlashLightController>();

            inventoryUI.OnActiveToolbarSlotUpdate += OnActiveToolbarSlotUpdate;

            inventoryUI.AddItemToInventory(new InventoryItemData(ItemType.AXE));
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
                if (!inventoryUI.ActiveToolbarSlot.IsEmpty())
                {
                    Transform cameraTransform = MouseLook.cameraTransform;
                    inventoryUI.DropToolbarItem(cameraTransform.position + cameraTransform.forward, 1, droppedItemVelocity);
                }
            }

            // flashlight
            if (Input.GetKeyDown(KeyCode.F))
                flashLightController.NextFlashLightMode();

            // TESTING
            if (Input.GetKeyDown(KeyCode.H))
            {
                inventoryUI.AddItemToInventory(new InventoryItemData(ItemType.PISTOL_M1911));
                inventoryUI.AddItemToInventory(new InventoryItemData(ItemType.RIFLE_AK74));
                inventoryUI.AddItemToInventory(new InventoryItemData(BlockType.OBSIDIAN, 100));
                inventoryUI.AddItemToInventory(new InventoryItemData(BlockType.STONE, 100));
            }

        }

        private void OnActiveToolbarSlotUpdate(InventoryItemData oldItem, InventoryItemData newItem)
        {
            if (!oldItem.IsSameItem(newItem))
            {
                LoadInHandModel();
            }
        }

        private void OnHandObjectPrefabLoaded(AsyncOperationHandle<GameObject> obj, ItemType itemType)
        {
            if (inventoryUI.ActiveToolbarSlot.ItemType != itemType)
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

            ItemType itemType = inventoryUI.ActiveToolbarSlot.ItemType;
            if (inventoryUI.ActiveToolbarSlot.IsTool(out InventoryItemTool inventoryItemTool) && inventoryItemTool.addressablePathToModel != string.Empty)
            {
                Addressables.LoadAssetAsync<GameObject>(inventoryItemTool.addressablePathToModel).Completed += ((AsyncOperationHandle<GameObject> obj) => OnHandObjectPrefabLoaded(obj, itemType));
            }
            else if (inventoryUI.ActiveToolbarSlot.IsWeapon(out InventoryItemWeapon inventoryItemWeapon) && inventoryItemWeapon.addressablePathToModel != string.Empty)
            {
                Addressables.LoadAssetAsync<GameObject>(inventoryItemWeapon.addressablePathToModel).Completed += ((AsyncOperationHandle<GameObject> obj) => OnHandObjectPrefabLoaded(obj, itemType));
            }
        }
    }
}