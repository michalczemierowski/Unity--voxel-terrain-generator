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

        public void OnBlockUpdate(BlockEventData data, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args)
        {
            // if above block is solid block
            if (WorldData.GetBlockState(neighbours[BlockFace.TOP].blockType) == BlockState.SOLID)
            {
                // replace current block with dirt in next update
                data.chunk.AddBlockToBuildList(data.LocalPosition, BlockType.DIRT);
            }
        }
    }
}
