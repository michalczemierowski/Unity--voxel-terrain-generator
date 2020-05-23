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
    public LayerMask groundLayer;

    public Inventory inv;

    public float maxDist = 8;
    public bool down = false;

    private void DestroySphere()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, maxDist, groundLayer))
        {
            Vector3Int pointInTargetBlock;

                pointInTargetBlock = Vector3Int.FloorToInt(hitInfo.point + transform.forward * .01f);//move a little inside the block

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

            Dictionary<Chunk, List<BlockData>> keyValuePairs = new Dictionary<Chunk, List<BlockData>>();
            foreach (var pos in positions)
            {
                Chunk chunk = World.GetChunk(pos.x, pos.z);
                if(!keyValuePairs.ContainsKey(chunk))
                    keyValuePairs.Add(chunk, new List<BlockData>());

                BlockPosition blockPos = new BlockPosition(pos.x, pos.y, pos.z);
                keyValuePairs[chunk].Add(new BlockData(BlockType.AIR, blockPos));
                //chunk.ClearParameters(blockPos);
            }

            foreach (var item in keyValuePairs)
            {
                item.Key.SetBlocksWithoutChecks(item.Value.ToArray());
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        bool leftClick = down ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0);
        bool rightClick = down ? Input.GetMouseButtonDown(1) : Input.GetMouseButton(1);

        if (Input.GetKeyDown(KeyCode.F))
            DestroySphere();

        if (leftClick || rightClick)
        { 
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, maxDist, groundLayer))
            {
                Vector3 pointInTargetBlock;

                //destroy
                if (leftClick)
                    pointInTargetBlock = hitInfo.point + transform.forward * .01f;//move a little inside the block
                else
                    pointInTargetBlock = hitInfo.point - transform.forward * .01f;

                BlockPosition blockPosition = new BlockPosition(Mathf.FloorToInt(pointInTargetBlock.x) + 1, Mathf.FloorToInt(pointInTargetBlock.y), Mathf.FloorToInt(pointInTargetBlock.z) + 1);
                Chunk tc = World.GetChunk(pointInTargetBlock.x, pointInTargetBlock.z);

                //replace block with air
                if (leftClick)
                {
                    tc.ClearParameters(blockPosition);
                    tc.SetBlock(blockPosition, BlockType.AIR, true);
                }
                else if (rightClick)
                {
                    // schedule update if below block is grass block (if above block is solid : grass_block -> dirt_block)
                    // TODO: checks
                    if (tc.GetBlock(blockPosition.Below()) == BlockType.GRASS_BLOCK)
                        World.ScheduleUpdate(tc, blockPosition.Below(), 10, 50);

                    Block block = WorldData.GetBlock(inv.GetCurrentBlock());

                    if (block.shape == BlockShape.HALF_BLOCK)
                        tc.SetParameters(new BlockParameter(blockPosition, ParameterType.ROTATION), (short)(Mathf.RoundToInt(transform.eulerAngles.y / 90) * 90));
                    if (block.type == BlockType.WATER)
                        tc.SetParameters(new BlockParameter(blockPosition, ParameterType.WATER_SOURCE_DISTANCE), (short)8);

                    tc.SetBlock(blockPosition, inv.GetCurrentBlock());
                }
            }
        }
    }
}
