using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using VoxelTG.Player.Interactions;
using VoxelTG.Player.Inventory;
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

        [SerializeField] private InventorySystem inventorySystem;
        public static InventorySystem InventorySystem => Instance.inventorySystem;

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

        private void OnEnable()
        {
            inventorySystem.OnMainHandUpdate += OnActiveToolbarSlotUpdate;
        }

        private void OnDestroy()
        {
            inventorySystem.OnMainHandUpdate -= OnActiveToolbarSlotUpdate;
        }

        private void Update()
        {
            if (AreControlsActive)
                HandleInput();
        }

        // TODO: input system
        private void HandleInput()
        {
            // drop item
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!inventorySystem.HandSlot.IsEmpty())
                {
                    Transform cameraTransform = MouseLook.cameraTransform;
                    inventorySystem.DropItem(inventorySystem.HandSlot, cameraTransform.position + cameraTransform.forward, 1, droppedItemVelocity);
                }
            }

            // flashlight
            if (Input.GetKeyDown(KeyCode.F))
                flashlightController.NextFlashLightMode();

            // TESTING
            if (Input.GetKeyDown(KeyCode.H))
            {
                // TODO: add items to inventory
                inventorySystem.AddItem(ItemType.AXE, 1);
                inventorySystem.AddItem(BlockType.OAK_LOG, 32);
            }

        }

        private void OnActiveToolbarSlotUpdate(InventorySlot oldContent, InventorySlot newContent)
        {
            if (!oldContent.Item.IsSameType(newContent.Item))
            {
                LoadInHandModel();
            }
        }

        private void OnHandObjectPrefabLoaded(AsyncOperationHandle<GameObject> obj, ItemType itemType)
        {
            if (inventorySystem.HandSlot.ItemType != itemType)
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

            ItemType itemType = inventorySystem.HandSlot.ItemType;
            if (inventorySystem.HandSlot.Item.IsTool())
            {
                InventoryItemTool inventoryItemTool = (InventoryItemTool)inventorySystem.HandSlot.Item;
                if (inventoryItemTool.addressablePathToModel != string.Empty)
                {
                    Addressables.LoadAssetAsync<GameObject>(inventoryItemTool.addressablePathToModel).Completed += (handle) =>
                    {
                        OnHandObjectPrefabLoaded(handle, itemType);
                    };
                }
            }
            else if (inventorySystem.HandSlot.Item.IsWeapon())
            {
                InventoryItemWeapon inventoryItemWeapon = (InventoryItemWeapon)inventorySystem.HandSlot.Item;
                if (inventoryItemWeapon.addressablePathToModel != string.Empty)
                {
                    Addressables.LoadAssetAsync<GameObject>(inventoryItemWeapon.addressablePathToModel).Completed += (handle) =>
                    {
                        OnHandObjectPrefabLoaded(handle, itemType);
                    };
                }
            }
        }
    }
}