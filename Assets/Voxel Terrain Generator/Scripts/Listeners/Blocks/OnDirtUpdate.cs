using System.Collections.Generic;
using System.Linq;
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

        public void OnBlockUpdate(BlockUpdateEventData data, Dictionary<BlockFace, BlockUpdateEventData> neighbours)
        {
            return;
        }
    }
}
