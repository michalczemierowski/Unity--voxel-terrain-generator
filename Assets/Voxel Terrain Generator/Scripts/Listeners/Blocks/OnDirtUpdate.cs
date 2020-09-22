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
    public class OnDirtUpdate : MonoBehaviour, IBlockUpdateListener
    {
        public BlockType GetBlockType()
        {
            return BlockType.DIRT;
        }

        public void OnBlockUpdate(BlockEventData data, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args)
        {
            BlockState aboveBlockState = WorldData.GetBlockState(neighbours[BlockFace.TOP].blockType);
            // if above block is not solid and there is grass block nearby - build grass block
            if (aboveBlockState != BlockState.SOLID &&
                aboveBlockState != BlockState.LIQUID && (
                neighbours[BlockFace.BACK].blockType == BlockType.GRASS_BLOCK ||
                neighbours[BlockFace.FRONT].blockType == BlockType.GRASS_BLOCK ||
                neighbours[BlockFace.LEFT].blockType == BlockType.GRASS_BLOCK ||
                neighbours[BlockFace.RIGHT].blockType == BlockType.GRASS_BLOCK))
            {
                // if args[0] == 1 build grass block
                if (args.Length > 0 && args[0] == 1)
                    data.chunk.AddBlockToBuildList(data.LocalPosition, BlockType.GRASS_BLOCK);
                else
                    // schedule grass build and pass args[0] = 1
                    World.ScheduleUpdate(data.chunk, data.LocalPosition, Random.Range(100, 200), 1);
            }
        }
    }
}
