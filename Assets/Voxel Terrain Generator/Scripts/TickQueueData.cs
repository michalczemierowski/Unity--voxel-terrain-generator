
public class TickQueueData
{
    public TerrainChunk chunk;
    public BlockPos blockPos;
    public int ticks;

    public TickQueueData(TerrainChunk chunk, BlockPos blockPos, int ticks)
    {
        this.chunk = chunk;
        this.blockPos = blockPos;
        this.ticks = ticks;
    }
}

