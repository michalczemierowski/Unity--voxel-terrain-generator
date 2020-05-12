using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TerrainModifier : MonoBehaviour
{
    public LayerMask groundLayer;

    public Inventory inv;

    float maxDist = 4;

    // Update is called once per frame
    void Update()
    {
        bool leftClick = Input.GetMouseButtonDown(0);
        bool rightClick = Input.GetMouseButtonDown(1);

        if(leftClick || rightClick) { 
        RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, maxDist, groundLayer))
            {
                Vector3 pointInTargetBlock;

                //destroy
                if (leftClick)
                    pointInTargetBlock = hitInfo.point + transform.forward * .01f;//move a little inside the block
                else
                    pointInTargetBlock = hitInfo.point - transform.forward * .01f;

                //get the terrain chunk (can't just use collider)
                int chunkPosX = Mathf.FloorToInt(pointInTargetBlock.x / TerrainChunk.chunkWidth) * TerrainChunk.chunkWidth;
                int chunkPosZ = Mathf.FloorToInt(pointInTargetBlock.z / TerrainChunk.chunkWidth) * TerrainChunk.chunkWidth;

                ChunkPos cp = new ChunkPos(chunkPosX, chunkPosZ);

                TerrainChunk tc = TerrainGenerator.chunks[cp];

                //index of the target block
                int bix = Mathf.FloorToInt(pointInTargetBlock.x) - chunkPosX + 1;
                int biy = Mathf.FloorToInt(pointInTargetBlock.y);
                int biz = Mathf.FloorToInt(pointInTargetBlock.z) - chunkPosZ + 1;

                if (leftClick)//replace block with air
                {
                    //TerrainGenerator.SetBlock(new BlockData(BlockType.Air, new Vector3Int(bix + chunkPosX - 1, biy, biz + chunkPosZ - 1)));
                    tc.ClearParameters(bix, biy, biz);
                    tc.UpdateBlock(bix, biy, biz, BlockType.AIR);
                    //tc.blocks[index] = BlockType.Air;
                    //tc.BuildMesh();
                    //CheckNeighbours(tc, bix, biy, biz, BlockType.Air, cp);
                }
                else if (rightClick)
                {
                    //TerrainGenerator.SetBlock(new BlockData(inv.GetCurBlock(), new Vector3Int(bix + chunkPosX - 1, biy, biz + chunkPosZ - 1)));
                    //if(inv.CanPlaceCur())
                    //{
                    //int index = TerrainChunk.Index3Dto1D(bix, biy, biz);
                    //BlockType blockType = inv.GetCurBlock();
                    //tc.blocks[index] = inv.GetCurBlock();

                    if (tc.GetBlock(bix, biy - 1, biz) == BlockType.GRASS_BLOCK)
                        TerrainGenerator.ScheduleUpdate(tc, new BlockPos(bix, biy - 1, biz), 10, 50);

                    Block block = TerrainData.GetBlock(inv.GetCurrentBlock());

                    if (block.shape == BlockShape.HALF_BLOCK)
                        tc.SetParameters(new BlockParameter(new int3(bix, biy, biz), ParameterType.ROTATION), (short)(Mathf.RoundToInt(transform.eulerAngles.y / 90) * 90));
                    if (block.type == BlockType.WATER)
                        tc.SetParameters(new BlockParameter(new int3(bix, biy, biz), ParameterType.WATER_SOURCE_DISTANCE), (short)8);

                    tc.UpdateBlock(bix, biy, biz, inv.GetCurrentBlock());
                    //tc.BuildMesh();
                    //CheckNeighbours(tc, bix, biy, biz, blockType, cp);

                    //    inv.ReduceCur();
                    //}
                }
            }
        }

    }
}
