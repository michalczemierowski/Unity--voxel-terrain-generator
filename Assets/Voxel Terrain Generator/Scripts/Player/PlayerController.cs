/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VoxelTG.Player.Inventory;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;
using VoxelTG.UI;

namespace VoxelTG.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float droppedItemVelocity = 3;
        [Header("References")]
        public PlayerMovement m_PlayerMovement;
        public MouseLook m_MouseLook;
        public TerrainInteractions m_TerrainInteractions;

        [NonSerialized]
        public UIManager uiManager;
        [NonSerialized]
        public InventoryUI inventoryUI;

        [SerializeField] private Transform handTransform;

        private int currentlySelectedToolbarSlot;

        private void Start()
        {
            uiManager = UIManager.Instance;
            inventoryUI = uiManager.inventoryUI;
        }

        private void Update()
        {
            HandleInput();
        }

        private void SelectToolbarSlot(int slot)
        {
            if (inventoryUI.OutOfToolbarRange(slot))
                return;

            inventoryUI.SelectToolbarSlot(slot);
            m_TerrainInteractions.ResetMining();

            if (currentlySelectedToolbarSlot != slot)
            {
                for (int j = 0; j < handTransform.childCount; j++)
                {
                    Destroy(handTransform.GetChild(j).gameObject);
                }

                if (inventoryUI.ActiveToolbarSlot.IsTool(out InventoryItemTool inventoryItemTool) && inventoryItemTool.addressablePathToModel != string.Empty)
                {
                    Addressables.LoadAssetAsync<GameObject>(inventoryItemTool.addressablePathToModel).Completed += OnHandObjectPrefabLoaded;
                }
            }

            currentlySelectedToolbarSlot = slot;
        }

        private void HandleInput()
        {
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown((KeyCode)(49 + i)))
                {
                    SelectToolbarSlot(i);
                }
            }

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                SelectToolbarSlot(currentlySelectedToolbarSlot - 1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                SelectToolbarSlot(currentlySelectedToolbarSlot + 1);
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (!inventoryUI.ActiveToolbarSlot.IsEmpty())
                {
                    Transform cameraTransform = MouseLook.cameraTransform;
                    inventoryUI.DropToolbarItem(cameraTransform.position + cameraTransform.forward, 1, droppedItemVelocity);
                }
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                inventoryUI.AddItemToInventory(new InventoryItemData(ToolType.PICKAXE));
                inventoryUI.AddItemToInventory(new InventoryItemData(ToolType.SWORD));
                inventoryUI.AddItemToInventory(new InventoryItemData(BlockType.OBSIDIAN, 32));
            }

        }

        private void OnHandObjectPrefabLoaded(AsyncOperationHandle<GameObject> obj)
        {
            GameObject prefab = obj.Result;
            if (prefab != null)
            {
                Instantiate(prefab, handTransform);
            }
        }
    }
}