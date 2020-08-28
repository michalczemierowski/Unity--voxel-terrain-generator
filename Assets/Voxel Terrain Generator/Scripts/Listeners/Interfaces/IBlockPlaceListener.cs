using System.Collections.Generic;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Listeners.Interfaces
{
    public interface IBlockPlaceListener
    {
        /// <summary>
        /// Method called at initiation of event listeners
        /// </summary>
        /// <returns>Block type you want to listen for updates</returns>
        BlockType GetBlockType();

        /// <summary>
        /// Method called when block is placed by player
        /// </summary>
        /// <param name="data">current block data</param>
        void OnBlockPlaced(BlockEventData data, params int[] args);
    }
}