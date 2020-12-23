using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VoxelTG.Entities.Items;
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
        [System.NonSerialized]
        public bool handlePlacingBlocks = false;
        [System.NonSerialized]
        public bool handleDestroyingBlocks = false;

        [SerializeField] private LayerMask groundLayer;
        [Space]
        [SerializeField] private GameObject selectedBlockOutlinePrefab;
        private GameObject selectedBlockOutline;

        private InventoryUI inventoryUI;

        public float maxInteractDistance { get; set; } = 8;
        private Transform cameraTransform;

        private BlockType lastSelectedBlock;

        private int3 miningBlockPosition;
        private float miningBlockMaxDurability;
        private float miningBlockDurability;

        private Image miningProgressImage;

        private void Start()
        {
            cameraTransform = Camera.main.transform;
            miningProgressImage = UIManager.MiningProgressImage;
            inventoryUI = UIManager.InventoryUI;

            inventoryUI.OnActiveToolbarSlotUpdate += OnActiveToolbarSlotUpdate;
            selectedBlockOutline = Instantiate(selectedBlockOutlinePrefab, Vector3.zero, Quaternion.identity);
        }

        private void OnActiveToolbarSlotUpdate(InventorySlot oldItem, InventorySlot newItem)
        {
            if (oldItem.IsSameType(newItem))
                return;

            if (newItem.IsMaterial())
            {
                handleDestroyingBlocks = false;
                handlePlacingBlocks = true;

                if (newItem.BlockType != lastSelectedBlock)
                {
                    lastSelectedBlock = newItem.BlockType;
                }
            }
            else if (newItem.IsTool())
            {
                handleDestroyingBlocks = true;
                handlePlacingBlocks = false;
            }
            else
            {
                handleDestroyingBlocks = false;
                handlePlacingBlocks = false;

                selectedBlockOutline.SetActive(false);
            }
        }

        private void Update()
        {
            if (PlayerController.AreControlsActive)
            {
                HandleInput();

                if (handleDestroyingBlocks || handlePlacingBlocks)
                {
                    HandleHover();
                }
            }
        }

        private void HandleInput()
        {
            bool inputDestroy = Input.GetMouseButton(0);
            bool inputPlace = Input.GetMouseButtonDown(1);

            if (handlePlacingBlocks && inputPlace && inventoryUI.SelectedToolbarSlot.IsMaterial())
            {
                HandlePlacing();
            }
            else if (handleDestroyingBlocks && (inputDestroy || inputPlace))
            {
                HandleDestroying(inputDestroy, inputPlace);
            }

            if (Input.GetMouseButtonDown(1))
            {
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

        private void HandleHover()
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, maxInteractDistance, groundLayer))
            {
                if (hitInfo.transform.CompareTag("Terrain"))
                {
                    Vector3 pointInTargetBlock;

                    // move towards block position
                    if (handlePlacingBlocks)
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
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, maxInteractDistance, groundLayer) && hitInfo.transform.CompareTag("Terrain"))
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
                Block block = WorldData.GetBlockData(inventoryUI.SelectedToolbarSlot.BlockType);

                if (block.shape == BlockShape.HALF_BLOCK)
                    chunk.SetParameters(new BlockParameter(blockPosition, ParameterType.ROTATION), (short)(Mathf.RoundToInt(cameraTransform.eulerAngles.y / 90) * 90));
                if (block.type == BlockType.WATER)
                    chunk.SetParameters(new BlockParameter(blockPosition, ParameterType.WATER_SOURCE_DISTANCE), 8);

                BlockType activeBlockType = inventoryUI.SelectedToolbarSlot.BlockType;

                chunk.SetBlock(blockPosition, activeBlockType, SetBlockSettings.PLACE);
                inventoryUI.UpdateToolbarItemCount(inventoryUI.SelectedToolbarSlot.PositionX, -1);
            }
        }

        private void HandleDestroying(bool inputDestroy, bool inputPlace)
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hitInfo, maxInteractDistance, groundLayer) && hitInfo.transform.CompareTag("Terrain"))
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
                    if (globalBlockPosition.Equals(miningBlockPosition))
                    {
                        miningBlockDurability -= Time.deltaTime;
                        miningProgressImage.fillAmount = miningBlockDurability / miningBlockMaxDurability;
                        if (miningBlockDurability <= 0)
                        {
                            chunk.ClearParameters(blockPosition);
                            // TODO: check if tool is correct
                            chunk.SetBlock(blockPosition, BlockType.AIR, SetBlockSettings.MINE);
                            miningBlockPosition = new int3(-1, -1, -1);
                        }
                    }
                    else
                    {
                        Block block = WorldData.GetBlockData(chunk.GetBlock(blockPosition));
                        miningBlockPosition = globalBlockPosition;
                        miningBlockMaxDurability = WorldData.GetBlockDurability(block.type) / inventoryUI.SelectedToolbarSlot.MiningSpeed;
                        miningBlockDurability = miningBlockMaxDurability;
                    }
                }
                else if (inputPlace)
                {
                    if (WorldData.CanPlaceGrass(chunk.GetBlock(blockPosition)) && chunk.GetBlock(blockPosition.Above()) == BlockType.GRASS)
                    {
                        chunk.SetBlock(blockPosition.Above(), BlockType.AIR, SetBlockSettings.DESTROY);
                    }
                }
            }
        }

        public void ResetMining()
        {
            miningBlockPosition = new int3(-1, -1, -1);
            miningProgressImage.fillAmount = 0;
        }
    }
}
