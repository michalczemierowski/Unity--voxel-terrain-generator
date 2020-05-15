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
    public class OnGrassBlockUpdate : MonoBehaviour, IBlockUpdateListener
    {
        public BlockType GetBlockType()
        {
            return BlockType.GRASS_BLOCK;
        }

        public void OnBlockUpdate(BlockUpdateEventData data, Dictionary<BlockFace, BlockUpdateEventData> neighbours)
        {
            if (WorldData.GetBlockState(neighbours[BlockFace.TOP].type) == BlockState.SOLID)
            {
                data.chunk.AddBlockToBuildList(data.position, BlockType.DIRT);
            }
        }
    }
}
