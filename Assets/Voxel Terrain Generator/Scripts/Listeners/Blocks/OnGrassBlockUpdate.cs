using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//https://github.com/michalczemierowski
public class OnGrassBlockUpdate : MonoBehaviour, IBlockUpdateListener
{
    public BlockType GetBlockType()
    {
        return BlockType.GRASS_BLOCK;
    }

    public void OnBlockUpdate(BlockUpdateEventData data, Dictionary<Side, BlockUpdateEventData> neighbours)
    {
        if (TerrainData.GetBlockState(neighbours[Side.TOP].type) == BlockState.SOLID)
        {
            data.chunk.AddBlockToBuildList(data.position, BlockType.DIRT);
        }
    }
}
