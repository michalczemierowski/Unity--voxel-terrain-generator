using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VoxelTG.Extensions;
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

        private GameObject objectInHand;
        private AsyncOperationHandle objectInHanHandle;
        public static GameObject ObjectInHand => Instance.objectInHand;

        public delegate void HandObjectLoaded(GameObject handObject, ItemType itemType);
        /// <summary>
        /// Called when object in hand is loaded successfully
        /// </summary>
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

            // player controller will be enabled when world loading is finished
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            inventorySystem.OnMainHandUpdate += OnMainHandUpdate;
        }

        private void OnDestroy()
        {
            inventorySystem.OnMainHandUpdate -= OnMainHandUpdate;
        }

        private void Start()
        {
            inventorySystem.AddItem(ItemType.AXE, 1);

            inventorySystem.AddItem(ItemType.RIFLE_AK74, 1);
            inventorySystem.AddItem(ItemType.PISTOL_M1911, 1);

            inventorySystem.AddItem(ItemType.AMMO_PISTOL, 100);

            inventorySystem.AddItem(BlockType.COBBLESTONE, 32);
            inventorySystem.AddItem(BlockType.DIRT, 32);
            inventorySystem.AddItem(BlockType.GRASS_BLOCK, 32);
            inventorySystem.AddItem(BlockType.OAK_LEAVES, 32);
            inventorySystem.AddItem(BlockType.OAK_LOG, 32);
            inventorySystem.AddItem(BlockType.OAK_PLANKS, 32);
            inventorySystem.AddItem(BlockType.OBSIDIAN, 32);
            inventorySystem.AddItem(BlockType.SAND, 32);
            inventorySystem.AddItem(BlockType.SPRUCE_LEAVES, 32);
            inventorySystem.AddItem(BlockType.SPRUCE_LOG, 32);
            inventorySystem.AddItem(BlockType.STONE, 32);
        }

        private void Update()
        {
            if (!UIManager.IsUIModeActive)
                HandleInGameInput();
            if (!UIManager.IsUsingUIInput)
                HandleAnyInput();
        }

        // TODO: input system
        private void HandleAnyInput()
        {
            // inventory
            if (Input.GetKeyDown(KeyCode.E))
            {
                UIManager.InventoryUI.ToggleUI();
            }

            // linked slots
            for (int i = 0; i < UIManager.InventoryUI.LinkedSlotsCount; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    var slot = UIManager.InventoryUI.GetLinkedSlot(i);
                    if (slot == null)
                        continue;

                    if (UIManager.IsUIModeActive)
                    {
                        // if hand slot isn't empty, try to link it
                        if (!inventorySystem.IsHandNullOrEmpty)
                            UIManager.InventoryUI.TryToLinkSlot(slot, inventorySystem.HandSlot);
                    }
                    else
                        inventorySystem.SetInHandSlot(slot.LinkedSlot);

                    break;
                }
            }

            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                DebugUtils.DebugManager.CommandHandler.ToggleUI();
            }

            // empty hand
            if (Input.GetKeyDown(KeyCode.X))
                inventorySystem.SetInHandSlot(null);

            // inputs available only in UI mode
            if (UIManager.IsUIModeActive)
            {
                // close active window when pressing Escape
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    UIManager.ToggleUIMode(false);
                }
            }
        }

        // TODO: input system
        private void HandleInGameInput()
        {
            // jumping
            if (Input.GetKeyDown(KeyCode.Space))
                m_PlayerMovement.TryToJump();

            // flying
            if (Input.GetKeyDown(KeyCode.Z))
                m_PlayerMovement.ToggleFlying();

            // drop item
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!inventorySystem.IsHandNullOrEmpty)
                {
                    Transform cameraTransform = MouseLook.cameraTransform;
                    inventorySystem.DropItem(inventorySystem.HandSlot, cameraTransform.position + cameraTransform.forward, 1, droppedItemVelocity);
                }
            }

            // flashlight
            if (Input.GetKeyDown(KeyCode.F))
                flashlightController.NextFlashLightMode();
        }

        private void OnMainHandUpdate(InventorySlot oldContent, InventorySlot newContent)
        {
            // if new item in hand is null, remove all items from hand
            if (newContent.IsNullOrEmpty())
            {
                RemoveInHandModel();
            }
            // else try to load model for item in hand
            else if (oldContent.IsNullOrEmpty() || !oldContent.Item.IsSameType(newContent.Item))
            {
                LoadInHandModel();
            }
        }

        /// <summary>
        /// Called when loading object in hand is finished
        /// </summary>
        private void OnHandObjectPrefabLoaded(AsyncOperationHandle<GameObject> obj, ItemType itemType)
        {
            // return if item has changed when object was loading
            if (inventorySystem.HandSlot.ItemType != itemType)
                return;

            GameObject prefab = obj.Result;
            if (prefab != null)
            {
                objectInHand = Instantiate(prefab, handTransform);
                OnHandObjectLoaded?.Invoke(objectInHand, itemType);
            }
        }

        /// <summary>
        /// Load prefab for object in hand (async loading, <see cref="OnHandObjectLoaded"/> will be called on complete)
        /// </summary>
        private void LoadInHandModel()
        {
            RemoveInHandModel();

            // don't need to load model for empty hand
            if (inventorySystem.IsHandNullOrEmpty)
                return;

            var item = inventorySystem.HandSlot.Item;
            if (item.AddressablePathToModel.RuntimeKeyIsValid())
            {
                ItemType itemType = item.Type;
                Addressables.LoadAssetAsync<GameObject>(item.AddressablePathToModel).Completed += (handle) =>
                {
                    objectInHanHandle = handle;
                    OnHandObjectPrefabLoaded(handle, itemType);
                };
            }
        }

        /// <summary>
        /// Just destroy all models in hand
        /// </summary>
        private void RemoveInHandModel()
        {
            if (objectInHanHandle.IsValid())
                Addressables.Release(objectInHanHandle);

            for (int j = 0; j < handTransform.childCount; j++)
            {
                Destroy(handTransform.GetChild(j).gameObject);
            }
        }
    }
}