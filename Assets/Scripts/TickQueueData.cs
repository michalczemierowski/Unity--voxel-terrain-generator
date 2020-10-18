using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public class TickQueueData
    {
        public Chunk chunk;
        public BlockPosition blockPos;
        public int ticks;
        public int[] args;

        public TickQueueData(Chunk chunk, BlockPosition blockPos, int ticks, params int[] args)
        {
            this.chunk = chunk;
            this.blockPos = blockPos;
            this.ticks = ticks;
            this.args = args;
        }
    }
}
