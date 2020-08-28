using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Listeners.Interfaces
{
    public interface IBlockArrayPlaceListener
    {
        /// <summary>
        /// Method called at initiation of event listeners
        /// </summary>
        /// <returns>Array of block types you want to listen for updates</returns>
        BlockType[] GetBlockTypes();

        /// <summary>
        /// Method called on block update
        /// </summary>
        /// <param name="data">current block data</param>
        void OnBlockPlaced(BlockEventData data, params int[] args);
    }
}