
using VoxelTG.Terrain.Blocks;

namespace VoxelTG.Terrain
{
    public class TickQueueData
    {
        public Chunk chunk;
        public BlockPosition blockPos;
        public int ticks;

        public TickQueueData(Chunk chunk, BlockPosition blockPos, int ticks)
        {
            this.chunk = chunk;
            this.blockPos = blockPos;
            this.ticks = ticks;
        }
    }
}
