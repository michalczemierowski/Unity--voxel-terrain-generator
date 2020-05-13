using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//https://github.com/michalczemierowski
public class OnWaterUpdate : MonoBehaviour, IBlockUpdateListener
{
    public BlockType GetBlockType()
    {
        return BlockType.WATER;
    }

    public void OnBlockUpdate(BlockUpdateEventData data, Dictionary<Side, BlockUpdateEventData> neighbours)
    {
        short sourceDistance = data.chunk.GetParameterValue(new BlockParameter(data.position, ParameterType.WATER_SOURCE_DISTANCE));

        BlockType belowBlock = neighbours[Side.DOWN].type;
        if (TerrainData.GetBlockState(belowBlock) == BlockState.SOLID)
        {
            for (int i = 0; i < 4; i++)
            {
                BlockUpdateEventData blockUpdateEventData = neighbours[(Side)i];
                TerrainChunk chunk = blockUpdateEventData.chunk;

                BlockParameter param = new BlockParameter(blockUpdateEventData.position, ParameterType.WATER_SOURCE_DISTANCE);

                int index = TerrainChunk.Index3Dto1D(blockUpdateEventData.position);
                BlockType type = chunk.blocks[index];

                if (sourceDistance > 0)
                {
                    if (type == BlockType.AIR || TerrainData.GetBlockState(type) == BlockState.PLANTS)
                    {
                        chunk.AddParameterToList(param, (short)(sourceDistance - 1));
                        chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, blockUpdateEventData.position));
                    }
                    if (type == BlockType.WATER)
                    {
                        if (chunk.GetParameterValue(param) < sourceDistance - 1)
                        {
                            chunk.AddParameterToList(param, (short)(sourceDistance - 1));
                            chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, blockUpdateEventData.position));
                        }
                    }
                }
            }
        }
        else if (belowBlock == BlockType.AIR || TerrainData.GetBlockState(belowBlock) == BlockState.PLANTS)
        {
            data.chunk.AddParameterToList(new BlockParameter(neighbours[Side.DOWN].position, ParameterType.WATER_SOURCE_DISTANCE), 8);
            data.chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, neighbours[Side.DOWN].position));
        }
    }
}
