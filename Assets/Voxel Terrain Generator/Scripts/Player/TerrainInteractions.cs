using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VoxelTG.Entities.Items;
using VoxelTG.Player.Inventory.Tools;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using VoxelTG.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player
{
    public class TerrainInteractions : MonoBehaviour
    {
        [SerializeField] private LayerMask groundLayer;
        [Space]
        [SerializeField] private GameObject selectedBlockCube;

        private InventoryUI inventoryUI;

        public float maxInteractDistance { get; set; } = 8;
        private Transform cameraTransform;

        private int3 miningBlockPosition;
        private float miningBlockMaxDurability;
        private float miningBlockDurability;

        private Image miningProgressImage;

        private Transform lastHoveredPickup;

        private void Start()
        {
            cameraTransform = Camera.main.transform;
            miningProgressImage = UIManager.Instance.miningProgressImage;
            inventoryUI = UIManager.Instance.inventoryUI;
        }

        private void Update()
        {
            HandleInput();

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, maxInteractDistance, groundLayer))
            {
                if (hitInfo.transform.CompareTag("Terrain"))
                {
                    Vector3 pointInTargetBlock;

                    // move towards block position
                    pointInTargetBlock = hitInfo.point + cameraTransform.forward * .01f;

                    // get block & chunk
                    Vector3 globalBlockPosition = new Vector3(Mathf.FloorToInt(pointInTargetBlock.x) + 1, Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z) + 1);
                    selectedBlockCube.SetActive(true);
                    selectedBlockCube.transform.position = globalBlockPosition;
                }
                else
                {
                    selectedBlockCube.SetActive(false);
                    if (miningProgressImage.fillAmount != 0)
                    {
                        miningBlockPosition = new int3(-1, -1, -1);
                        miningProgressImage.fillAmount = 0;
                    }
                }

                if (hitInfo.transform.CompareTag("Pickup"))
                {
                    if (lastHoveredPickup != hitInfo.transform)
                    {
                        if (lastHoveredPickup != null)
                            OnPickupHoverEnd(lastHoveredPickup);

                        lastHoveredPickup = hitInfo.transform;
                        OnPickupHoverStart(lastHoveredPickup);
                    }
                }
                else if (lastHoveredPickup != null)
                {
                    OnPickupHoverEnd(lastHoveredPickup);
                    lastHoveredPickup = null;
                }
            }
            else
                selectedBlockCube.SetActive(false);
        }

        private void OnPickupHoverStart(Transform pickup)
        {
            pickup.GetComponent<MeshRenderer>().material.SetFloat("_hover", 1);
        }

        private void OnPickupHoverEnd(Transform pickup)
        {
            pickup.GetComponent<MeshRenderer>().material.SetFloat("_hover", 0);
        }

        private void HandleInput()
        {
            bool inputDestroy = Input.GetMouseButton(0);
            bool inputPlace = Input.GetMouseButtonDown(1);

            if (inputPlace && inventoryUI.ActiveToolbarSlot.IsMaterial())
            {
                // if ray hits terrain
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, maxInteractDistance, groundLayer) && hitInfo.transform.CompareTag("Terrain"))
                {
                    Vector3 pointInTargetBlock;

                    // move towards block position
                    pointInTargetBlock = hitInfo.point - cameraTransform.forward * .01f;

                    // get block & chunk
                    int3 globalBlockPosition = new int3(Mathf.FloorToInt(pointInTargetBlock.x) + 1, Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z) + 1);
                    BlockPosition blockPosition = new BlockPosition(globalBlockPosition);
                    Chunk chunk = World.GetChunk(globalBlockPosition.x, globalBlockPosition.z);

                    // get selected block type from inventory
                    Block block = WorldData.GetBlockData(inventoryUI.ActiveToolbarSlot.BlockType);

                    if (block.shape == BlockShape.HALF_BLOCK)
                        chunk.SetParameters(new BlockParameter(blockPosition, ParameterType.ROTATION), (short)(Mathf.RoundToInt(cameraTransform.eulerAngles.y / 90) * 90));
                    if (block.type == BlockType.WATER)
                        chunk.SetParameters(new BlockParameter(blockPosition, ParameterType.WATER_SOURCE_DISTANCE), 8);

                    chunk.SetBlock(blockPosition, inventoryUI.ActiveToolbarSlot.BlockType);
                    inventoryUI.UpdateToolbarItemCount(inventoryUI.ActiveToolbarSlot.positionX, -1);
                }
            }
            else if (inputDestroy)
            {
                // if ray hits terrain
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, maxInteractDistance, groundLayer) && hitInfo.transform.CompareTag("Terrain"))
                {
                    Vector3 pointInTargetBlock;

                    // move towards block position
                    pointInTargetBlock = hitInfo.point + cameraTransform.forward * .01f;

                    // get block & chunk
                    int3 globalBlockPosition = new int3(Mathf.FloorToInt(pointInTargetBlock.x) + 1, Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z) + 1);
                    BlockPosition blockPosition = new BlockPosition(globalBlockPosition);
                    Chunk chunk = World.GetChunk(globalBlockPosition.x, globalBlockPosition.z);

                    if (globalBlockPosition.Equals(miningBlockPosition))
                    {
                        miningBlockDurability -= Time.deltaTime;
                        miningProgressImage.fillAmount = miningBlockDurability / miningBlockMaxDurability;
                        if (miningBlockDurability <= 0)
                        {
                            BlockType blockType = chunk.GetBlock(blockPosition);

                            chunk.ClearParameters(blockPosition);
                            chunk.SetBlock(blockPosition, BlockType.AIR, true);
                            miningBlockPosition = new int3(-1, -1, -1);

                            // TODO: check if tool is correct
                            Vector3 droppedItemPosition = new Vector3(globalBlockPosition.x - 0.5f, globalBlockPosition.y + 0.5f, globalBlockPosition.z - 0.5f);
                            DroppedItemsManager.Instance.DropMaterial(blockType, droppedItemPosition);
                        }
                    }
                    else
                    {
                        Block block = WorldData.GetBlockData(chunk.GetBlock(blockPosition));
                        miningBlockPosition = globalBlockPosition;
                        miningBlockMaxDurability = ToolUtils.GetBlockDurability(block.type, inventoryUI.ActiveToolbarSlot.ToolType);
                        miningBlockDurability = miningBlockMaxDurability;
                    }

                    // if (block.shape == BlockShape.HALF_BLOCK)
                    //     chunk.SetParameters(new BlockParameter(blockPosition, ParameterType.ROTATION), (short)(Mathf.RoundToInt(cameraTransform.eulerAngles.y / 90) * 90));
                    // if (block.type == BlockType.WATER)
                    //     chunk.SetParameters(new BlockParameter(blockPosition, ParameterType.WATER_SOURCE_DISTANCE), 8);

                    // chunk.SetBlock(blockPosition, inventory.GetCurrentBlock());
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                // if ray hits terrain
                if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, maxInteractDistance, groundLayer))
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

        public void ResetMining()
        {
            miningBlockPosition = new int3(-1, -1, -1);
            miningProgressImage.fillAmount = 0;
        }
    }
}
