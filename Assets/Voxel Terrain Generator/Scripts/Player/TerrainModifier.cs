using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
public class TerrainModifier : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Inventory inventory;

    public float maxInteractDistance { get; set; } = 8;
    public bool down { get; set; } = true;

    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        bool inputDestroy = down ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0);
        bool inputPlace = down ? Input.GetMouseButtonDown(1) : Input.GetMouseButton(1);

        // experimental
        if (Input.GetKeyDown(KeyCode.F))
            DestroySphere();

        if (inputDestroy || inputPlace)
        {
            RaycastHit hitInfo;
            // if ray hits terrain
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo, maxInteractDistance, groundLayer))
            {
                Vector3 pointInTargetBlock;

                // move towards block position
                if (inputDestroy)
                    pointInTargetBlock = hitInfo.point + cameraTransform.forward * .01f;
                else
                    pointInTargetBlock = hitInfo.point - cameraTransform.forward * .01f;

                // get block & chunk
                int3 globalBlockPosition = new int3(Mathf.FloorToInt(pointInTargetBlock.x) + 1, Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z) + 1);
                BlockPosition blockPosition = new BlockPosition(globalBlockPosition);
                Chunk chunk = World.GetChunk(globalBlockPosition.x, globalBlockPosition.z);

                if (inputDestroy)
                {
                    // clear block parameters and remove block
                    chunk.ClearParameters(blockPosition);
                    chunk.SetBlock(blockPosition, BlockType.AIR, true);
                }
                else if (inputPlace)
                {
                    // schedule update if below block is grass block (if above block is solid : grass_block -> dirt_block)
                    if (chunk.GetBlock(blockPosition.Below()) == BlockType.GRASS_BLOCK)
                        World.ScheduleUpdate(chunk, blockPosition.Below(), 10, 50);

                    // get selected block type from inventory
                    Block block = WorldData.GetBlock(inventory.GetCurrentBlock());

                    if (block.shape == BlockShape.HALF_BLOCK)
                        chunk.SetParameters(new BlockParameter(blockPosition, ParameterType.ROTATION), (short)(Mathf.RoundToInt(cameraTransform.eulerAngles.y / 90) * 90));
                    if (block.type == BlockType.WATER)
                        chunk.SetParameters(new BlockParameter(blockPosition, ParameterType.WATER_SOURCE_DISTANCE), 8);

                    chunk.SetBlock(blockPosition, inventory.GetCurrentBlock());
                }
            }
        }
    }

    #region experimental

    // TODO: out of range errors
    private void DestroySphere()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hitInfo, maxInteractDistance, groundLayer))
        {
            Vector3Int pointInTargetBlock;

            pointInTargetBlock = Vector3Int.FloorToInt(hitInfo.point + cameraTransform.forward * .01f);//move a little inside the block

            Vector3Int blockPosition = new Vector3Int(pointInTargetBlock.x + 1, pointInTargetBlock.y, pointInTargetBlock.z + 1);

            List<Vector3Int> positions = new List<Vector3Int>();

            int maxdist = 8;
            for (int x = -maxdist; x < maxdist; x++)
            {
                for (int y = -maxdist; y < maxdist; y++)
                {
                    for (int z = -maxdist; z < maxdist; z++)
                    {
                        Vector3Int np = new Vector3Int(blockPosition.x + x, blockPosition.y + y, blockPosition.z + z);
                        if (Vector3Int.Distance(blockPosition, np) < maxdist)
                            positions.Add(np);
                    }
                }
            }

            BlockType type = inventory.GetCurrentBlock();
            Dictionary<Chunk, List<BlockData>> keyValuePairs = new Dictionary<Chunk, List<BlockData>>();
            foreach (var pos in positions)
            {
                Chunk chunk = World.GetChunk(pos.x, pos.z);
                if (!keyValuePairs.ContainsKey(chunk))
                    keyValuePairs.Add(chunk, new List<BlockData>());

                BlockPosition blockPos = new BlockPosition(pos.x, pos.y, pos.z);
                keyValuePairs[chunk].Add(new BlockData(type, blockPos));
                //chunk.ClearParameters(blockPos);
            }

            foreach (var item in keyValuePairs)
            {
                item.Key.SetBlocksWithoutChecks(item.Value.ToArray());
            }
        }
    }

    #endregion
}
