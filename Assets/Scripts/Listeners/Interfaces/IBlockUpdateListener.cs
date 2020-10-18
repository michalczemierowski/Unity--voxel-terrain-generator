using System.Collections.Generic;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Listeners.Interfaces
{
    public interface IBlockUpdateListener
    {
        /// <summary>
        /// Method called at initiation of event listeners
        /// </summary>
        /// <returns>Block type you want to listen for updates</returns>
        BlockType GetBlockType();

        /// <summary>
        /// Method called on block update
        /// </summary>
        /// <param name="data">current block data</param>
        /// <param name="neighbours">neighbour blocks data</param>
        void OnBlockUpdate(BlockEventData data, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args);
    }
}