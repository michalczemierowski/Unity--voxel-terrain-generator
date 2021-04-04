using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VoxelTG.Entities.Items;
using VoxelTG.Extensions;
using VoxelTG.Player.Inventory;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Interactions
{
    public class PlayerTerrainInteractions : MonoBehaviour
    {
        public bool HandlePlacingBlocks { get; private set; } = false;
        public bool HandleDestroyingBlocks { get; private set; } = false;

        private float maxInteractDistance = 8;
        public float MaxInteractDistance
        {
            get => maxInteractDistance;
            set
            {
                maxInteractDistance = value > 0 ? value : 1;
            }
        }

        /// <summary>
        /// Type of currently used material or last used material if player is not using it right now
        /// </summary>
        public BlockType LastUsedMaterial { get; private set; }

        [Tooltip("Layers on which interactions are possible")]
        [SerializeField] private LayerMask interactionLayers;
        /// <summary>
        /// Layers on which interactions are possible
        /// </summary>
        public LayerMask InteractionLayers => interactionLayers;

        [Header("References")]
        [SerializeField] private GameObject selectedBlockOutlinePrefab;

        private GameObject selectedBlockOutline;
        private Transform cameraTransform;

        private int3 miningBlockPosition;
        private float miningBlockMaxDurability;
        private float miningBlockDurability;

        // TODO: handle this in UI manager
        private Image miningProgressImage;

        private void Start()
        {
            cameraTransform = Camera.main.transform;
            miningProgressImage = UIManager.MiningProgressImage;

            selectedBlockOutline = Instantiate(selectedBlockOutlinePrefab, Vector3.zero, Quaternion.identity);

            PlayerController.InventorySystem.OnMainHandUpdate += OnMainHandUpdate;
            PlayerController.Instance.OnHandObjectLoaded += OnHandObjectLoaded;
        }

        private void OnMainHandUpdate(InventorySlot oldContent, InventorySlot newContent)
        {
            // if old slot is same as new - return
            if (!oldContent.IsNullOrEmpty() && !newContent.IsNullOrEmpty() && oldContent.Item.IsSameType(newContent.Item))
                return;

            if (!newContent.IsNullOrEmpty())
            {
                // enable material logic
                if (newContent.Item.IsMaterial)
                {
                    HandleDestroyingBlocks = false;
                    HandlePlacingBlocks = true;

                    if (newContent.BlockType != LastUsedMaterial)
                    {
                        LastUsedMaterial = newContent.BlockType;
                    }
                    return;
                }

                // enable tool logic
                if (newContent.Item.IsTool)
                {
                    HandleDestroyingBlocks = true;
                    HandlePlacingBlocks = false;
                    return;
                }
            }

            // disallow interactions if any of above is true
            HandleDestroyingBlocks = false;
            HandlePlacingBlocks = false;

            selectedBlockOutline.SetActive(false);
        }

        private void OnHandObjectLoaded(GameObject handObject, ItemType itemType)
        {
            if (itemType != ItemType.MATERIAL || handObject == null)
                return;

            // search for object named "block_cube" and apply block settings
            for (int i = 0; i < handObject.transform.childCount; i++)
            {
                Transform child = handObject.transform.GetChild(i);
                if (child.name.Equals(WorldSettings.Textures.BlockCubeName) && child.TryGetComponent(out MeshFilter meshFilter))
                {
                    MeshUtils.CreateBlockCube(meshFilter.mesh, LastUsedMaterial, 1);
                    meshFilter.ClearMeshOnDestroy();
                }
            }
        }

        private void Update()
        {
            if (!UIManager.IsUIModeActive)
            {
                HandleInput();

                if (HandleDestroyingBlocks || HandlePlacingBlocks)
                {
                    HandleHover();
                }
            }
        }

        // TODO: input system
        private void HandleInput()
        {
            bool inputDestroy = Input.GetMouseButton(0);
            bool inputPlace = Input.GetMouseButtonDown(1);

            if (HandlePlacingBlocks && inputPlace && PlayerController.InventorySystem.HandSlot.Item.IsMaterial)
            {
                HandlePlacing();
            }
            else if (HandleDestroyingBlocks && (inputDestroy || inputPlace))
            {
                HandleDestroying(inputDestroy, inputPlace);
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, MaxInteractDistance, interactionLayers))
                {
                    if (hitInfo.transform.CompareTag("Pickup"))
                    {
                        hitInfo.transform.GetComponent<DroppedItem>().Pickup();
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                miningBlockPosition = new int3(-1, -1, -1);
                miningProgressImage.fillAmount = 0;
            }
        }

        private void HandleHover()
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, MaxInteractDistance, interactionLayers))
            {
                if (hitInfo.transform.CompareTag("Terrain"))
                {
                    Vector3 pointInTargetBlock;

                    // move towards block position
                    if (HandlePlacingBlocks)
                        pointInTargetBlock = hitInfo.point - cameraTransform.forward * .01f;
                    else
                        pointInTargetBlock = hitInfo.point + cameraTransform.forward * .01f;

                    // get block & chunk
                    Vector3 globalBlockPosition = new Vector3(Mathf.FloorToInt(pointInTargetBlock.x) + 1, Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z) + 1);

                    selectedBlockOutline.SetActive(true);
                    selectedBlockOutline.transform.position = globalBlockPosition;
                }
                else
                {
                    selectedBlockOutline.SetActive(false);
                    if (miningProgressImage.fillAmount != 0)
                    {
                        miningBlockPosition = new int3(-1, -1, -1);
                        miningProgressImage.fillAmount = 0;
                    }
                }
            }
            else
                selectedBlockOutline.SetActive(false);
        }

        private void HandlePlacing()
        {
            // return if item in hand is not material
            if (!PlayerController.InventorySystem.HandSlot.Item.IsMaterial)
                return;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, MaxInteractDistance, interactionLayers) && hitInfo.transform.CompareTag("Terrain"))
            {
                Vector3 pointInTargetBlock;

                // move towards block position
                pointInTargetBlock = hitInfo.point - cameraTransform.forward * .01f;

                // get block & chunk
                int3 globalBlockPosition = new int3(Mathf.FloorToInt(pointInTargetBlock.x), Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z));
                BlockPosition blockPosition = new BlockPosition(globalBlockPosition);
                Chunk chunk = World.GetChunk(globalBlockPosition.x, globalBlockPosition.z);

                float distY = math.abs(transform.position.y - globalBlockPosition.y);

                bool sameX = globalBlockPosition.x == Mathf.FloorToInt(transform.position.x);
                bool sameY = (transform.position.y > globalBlockPosition.y) ? distY < 1.25f : distY < 0.25f;
                bool sameZ = globalBlockPosition.z == Mathf.FloorToInt(transform.position.z);

                if (sameX && sameY && sameZ)
                    return;

                // get selected block type from inventory
                InventoryItemMaterial inventoryItemMaterial = (InventoryItemMaterial)PlayerController.InventorySystem.HandSlot.Item;
                BlockStructure block = WorldData.GetBlockData(inventoryItemMaterial.BlockType);

                //if (block.shape == BlockShape.HALF_BLOCK)
                //    chunk.SetBlockParameter(new BlockParameter(blockPosition, ParameterType.ROTATION), (short)(Mathf.RoundToInt(cameraTransform.eulerAngles.y / 90) * 90));

                BlockType activeBlockType = inventoryItemMaterial.BlockType;

                chunk.SetBlock(blockPosition, activeBlockType, SetBlockSettings.PLACE);
                PlayerController.InventorySystem.RemoveItem(PlayerController.InventorySystem.HandSlot, 1);
            }
        }

        private void HandleDestroying(bool inputDestroy, bool inputPlace)
        {
            // return if item in hand is not tool
            if (!PlayerController.InventorySystem.HandSlot.Item.IsTool)
                return;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, MaxInteractDistance, interactionLayers) && hitInfo.transform.CompareTag("Terrain"))
            {
                Vector3 pointInTargetBlock;

                // move towards block position
                pointInTargetBlock = hitInfo.point + cameraTransform.forward * .01f;

                // get block & chunk
                int3 globalBlockPosition = new int3(Mathf.FloorToInt(pointInTargetBlock.x), Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z));
                BlockPosition blockPosition = new BlockPosition(globalBlockPosition);
                Chunk chunk = World.GetChunk(globalBlockPosition.x, globalBlockPosition.z);

                if (inputDestroy)
                {
                    // if player is mining same block
                    if (globalBlockPosition.Equals(miningBlockPosition))
                    {
                        miningBlockDurability -= Time.deltaTime;
                        miningProgressImage.fillAmount = miningBlockDurability / miningBlockMaxDurability;

                        if (miningBlockDurability <= 0)
                            OnBlockDestroyed(chunk, blockPosition);
                    }
                    else
                    {
                        var tool = (InventoryItemTool)PlayerController.InventorySystem.HandSlot.Item;
                        BlockStructure block = WorldData.GetBlockData(chunk.GetBlock(blockPosition));

                        miningBlockPosition = globalBlockPosition;
                        miningBlockMaxDurability = WorldData.GetBlockDurability(block.type) / tool.MiningSpeed;
                        miningBlockDurability = miningBlockMaxDurability;
                    }
                }
                else if (inputPlace)
                {
                    // remove grass when right clicking on it with tool
                    if (chunk.GetBlock(blockPosition.Above()) == BlockType.GRASS)
                    {
                        chunk.SetBlock(blockPosition.Above(), BlockType.AIR, SetBlockSettings.DESTROY);
                    }
                }
            }
        }

        /// <summary>
        /// Called when player destroys block using tool
        /// </summary>
        private void OnBlockDestroyed(Chunk chunk, BlockPosition blockPosition)
        {
            // remove parameters from destroyed block
            chunk.RemoveParameterAt(blockPosition);
            // replace block with air and drop item (drop item because SetBlockSettings = SetBlockSettings.MINE)
            chunk.SetBlock(blockPosition, BlockType.AIR, SetBlockSettings.MINE);
            // reset active mining block position
            miningBlockPosition = new int3(-1, -1, -1);

            // substract 1 from tool's durability
            var slot = PlayerController.InventorySystem.HandSlot;
            if (slot.TryGetMedadata<int>(InventoryItemTool.DURABILITY_MDK, out MetadataProperty<int> data))
            {
                int newValue = data.Value - 1;

                // remove 1 item if destroyed
                if (newValue <= 0)
                    PlayerController.InventorySystem.RemoveItem(slot, 1);
                // else, decrease durability by 1
                else
                    slot.SetMetadata(new MetadataProperty<int>(data.Key, newValue));
            }
        }

        public void ResetMining()
        {
            miningBlockPosition = new int3(-1, -1, -1);
            miningProgressImage.fillAmount = 0;
        }
    }
}
