using System.Collections.Generic;
using UnityEngine;
using VoxelTG.Listeners.Interfaces;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Blocks.Listeners
{
    public class OnWaterUpdate : MonoBehaviour, IBlockUpdateListener
    {
        public BlockType GetBlockType()
        {
            return BlockType.WATER;
        }


        // TODO: some cleaning?
        public void OnBlockUpdate(BlockEventData data, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args)
        {
            short sourceDistance = data.chunk.GetParameterValue(new BlockParameter(data.LocalPosition, ParameterType.WATER_SOURCE_DISTANCE));

            BlockType belowBlock = neighbours[BlockFace.BOTTOM].blockType;

            // checl for WATER_SOURCE_DISTANCE in neighbour blocks
            short biggestSourceDistance = 0;
            for (int i = 2; i < 6; i++)
            {
                BlockEventData blockUpdateEventData = neighbours[(BlockFace)i];
                int index = Utils.BlockPosition3DtoIndex(blockUpdateEventData.LocalPosition);
                BlockType type = blockUpdateEventData.chunk.blocks[index];

                if(type == BlockType.WATER)
                {
                    BlockParameter param = new BlockParameter(blockUpdateEventData.LocalPosition, ParameterType.WATER_SOURCE_DISTANCE);
                    short neighbourSourceDistance = blockUpdateEventData.chunk.GetParameterValue(param);
                    if(neighbourSourceDistance > biggestSourceDistance)
                        biggestSourceDistance = neighbourSourceDistance;
                }
            }

            if (WorldData.GetBlockState(belowBlock) == BlockState.SOLID)
            {
                // check only side blocks
                for (int i = 2; i < 6; i++)
                {
                    BlockEventData blockUpdateEventData = neighbours[(BlockFace)i];
                    Chunk chunk = blockUpdateEventData.chunk;

                    BlockParameter param = new BlockParameter(blockUpdateEventData.LocalPosition, ParameterType.WATER_SOURCE_DISTANCE);

                    int index = Utils.BlockPosition3DtoIndex(blockUpdateEventData.LocalPosition);
                    BlockType type = chunk.blocks[index];

                    if (sourceDistance > 0)
                    {
                        if (type == BlockType.AIR || WorldData.GetBlockState(type) == BlockState.LIQUID_DESTROYABLE)
                        {
                            chunk.AddParameterToList(param, (short)(sourceDistance - 1));
                            chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, blockUpdateEventData.LocalPosition));
                            
                            World.ScheduleUpdate(chunk, blockUpdateEventData.LocalPosition, 2);
                        }
                        if (type == BlockType.WATER)
                        {
                            // if neighbour block is WATER and neighbour sourceDistance is smaller, update neighbour sourceDistance
                            if (chunk.GetParameterValue(param) < sourceDistance - 1)
                            {
                                chunk.AddParameterToList(param, (short)(sourceDistance - 1));
                                chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, blockUpdateEventData.LocalPosition));
                            }
                        }
                    }
                }
            }
            // if block below is AIR or WATER or PLANTS, replace block with full WATER block
            else if (belowBlock == BlockType.AIR || belowBlock == BlockType.WATER || WorldData.GetBlockState(belowBlock) == BlockState.LIQUID_DESTROYABLE)
            {
                data.chunk.AddParameterToList(new BlockParameter(neighbours[BlockFace.BOTTOM].LocalPosition, ParameterType.WATER_SOURCE_DISTANCE), 8);
                data.chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, neighbours[BlockFace.BOTTOM].LocalPosition));
            }

            // if water is not full block, check if has neighbour with bigger SourceDistance
            if(sourceDistance != 8 && biggestSourceDistance <= sourceDistance)
            {
                // if sourceDistance == 0, replace block with air
                if(sourceDistance == 0)
                {
                    data.chunk.ClearParameters(data.LocalPosition);
                    data.chunk.AddBlockToBuildList(new BlockData(BlockType.AIR, data.LocalPosition));
                    
                    if(belowBlock == BlockType.WATER)
                    {
                        BlockEventData blockUpdateEventData = neighbours[BlockFace.BOTTOM];
                        data.chunk.AddParameterToList(new BlockParameter(blockUpdateEventData.LocalPosition, ParameterType.WATER_SOURCE_DISTANCE), (short)7);
                        World.ScheduleUpdate(blockUpdateEventData.chunk, blockUpdateEventData.LocalPosition, 1);
                    }
                }
                // else, decrease sourceDistance by 1
                else
                {
                    data.chunk.AddParameterToList(new BlockParameter(data.LocalPosition, ParameterType.WATER_SOURCE_DISTANCE), (short)(sourceDistance-1));
                    data.chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, data.LocalPosition));
                }
            }
        }
    }
}