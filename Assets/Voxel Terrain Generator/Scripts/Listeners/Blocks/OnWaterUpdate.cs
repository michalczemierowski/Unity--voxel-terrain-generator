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

        // TODO: rework
        public void OnBlockUpdate(BlockEventData data, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args)
        {
            short sourceDistance = data.chunk.GetParameterValue(new BlockParameter(data.LocalPosition, ParameterType.WATER_SOURCE_DISTANCE));

            BlockType belowBlock = neighbours[BlockFace.BOTTOM].blockType;
            if (WorldData.GetBlockState(belowBlock) == BlockState.SOLID)
            {
                for (int i = 2; i < 6; i++)
                {
                    BlockEventData blockUpdateEventData = neighbours[(BlockFace)i];
                    Chunk chunk = blockUpdateEventData.chunk;

                    BlockParameter param = new BlockParameter(blockUpdateEventData.LocalPosition, ParameterType.WATER_SOURCE_DISTANCE);

                    int index = Utils.BlockPosition3DtoIndex(blockUpdateEventData.LocalPosition);
                    BlockType type = chunk.blocks[index];

                    if (sourceDistance > 0)
                    {
                        if (type == BlockType.AIR || WorldData.GetBlockState(type) == BlockState.PLANTS)
                        {
                            chunk.AddParameterToList(param, (short)(sourceDistance - 1));
                            chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, blockUpdateEventData.LocalPosition));
                        }
                        if (type == BlockType.WATER)
                        {
                            if (chunk.GetParameterValue(param) < sourceDistance - 1)
                            {
                                chunk.AddParameterToList(param, (short)(sourceDistance - 1));
                                chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, blockUpdateEventData.LocalPosition));
                            }
                        }
                    }
                }
            }
            else if (belowBlock == BlockType.AIR || WorldData.GetBlockState(belowBlock) == BlockState.PLANTS)
            {
                data.chunk.AddParameterToList(new BlockParameter(neighbours[BlockFace.BOTTOM].LocalPosition, ParameterType.WATER_SOURCE_DISTANCE), 8);
                data.chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, neighbours[BlockFace.BOTTOM].LocalPosition));
            }
            else if(belowBlock == BlockType.WATER)
            {
                data.chunk.AddParameterToList(new BlockParameter(neighbours[BlockFace.BOTTOM].LocalPosition, ParameterType.WATER_SOURCE_DISTANCE), 8);
            }
        }
    }
}