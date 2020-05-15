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
    public bool down = true;

    // Update is called once per frame
    void Update()
    {
        bool leftClick = down ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0);
        bool rightClick = down ? Input.GetMouseButtonDown(1) : Input.GetMouseButton(1);

        if (leftClick || rightClick) { 
        RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, transform.forward, out hitInfo, maxDist, groundLayer))
            {
                Vector3 pointInTargetBlock;

                //destroy
                if (leftClick)
                    pointInTargetBlock = hitInfo.point + transform.forward * .01f;//move a little inside the block
                else
                    pointInTargetBlock = hitInfo.point - transform.forward * .01f;

                Chunk tc = World.GetChunkByBlockPosition(pointInTargetBlock.x, pointInTargetBlock.z);//World.chunks[cp];
                //BlockPosition blockPosition = new BlockPosition(pointInTargetBlock.x, pointInTargetBlock.y, pointInTargetBlock.z);
                //index of the target block
                int bix = Mathf.FloorToInt(pointInTargetBlock.x) - tc.chunkPos.x + 1;
                int biy = Mathf.FloorToInt(pointInTargetBlock.y);
                int biz = Mathf.FloorToInt(pointInTargetBlock.z) - tc.chunkPos.z + 1;

                if (leftClick)//replace block with air
                {
                    //TerrainGenerator.SetBlock(new BlockData(BlockType.Air, new Vector3Int(bix + chunkPosX - 1, biy, biz + chunkPosZ - 1)));
                    tc.ClearParameters(bix, biy, biz);
                    tc.SetBlock(bix, biy, biz, BlockType.AIR);
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

                    // schedule update if below block is grass block (if above block is solid : grass_block -> dirt_block)
                    if (tc.GetBlock(bix, biy - 1, biz) == BlockType.GRASS_BLOCK)
                        World.ScheduleUpdate(tc, new BlockPosition(bix, biy - 1, biz), 10, 50);

                    Block block = WorldData.GetBlock(inv.GetCurrentBlock());

                    if (block.shape == BlockShape.HALF_BLOCK)
                        tc.SetParameters(new BlockParameter(new int3(bix, biy, biz), ParameterType.ROTATION), (short)(Mathf.RoundToInt(transform.eulerAngles.y / 90) * 90));
                    if (block.type == BlockType.WATER)
                        tc.SetParameters(new BlockParameter(new int3(bix, biy, biz), ParameterType.WATER_SOURCE_DISTANCE), (short)8);

                    tc.SetBlock(bix, biy, biz, inv.GetCurrentBlock());
                    //tc.BuildMesh();
                    //CheckNeighbours(tc, bix, biy, biz, blockType, cp);

                    //    inv.ReduceCur();
                    //}
                }
            }
        }

    }
}
