using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public class WorldEventQueueData
    {
        /// <summary>
        /// Chunk on whick update will be called
        /// </summary>
        public Chunk chunk;
        /// <summary>
        /// Target block position
        /// </summary>
        public BlockPosition blockPosition;
        /// <summary>
        /// Event delay (in ticks)
        /// </summary>
        public int ticks;
        public int[] args;

        public WorldEventQueueData(Chunk chunk, BlockPosition blockPos, int ticks, params int[] args)
        {
            this.chunk = chunk;
            this.blockPosition = blockPos;
            this.ticks = ticks;
            this.args = args;
        }
    }
}
