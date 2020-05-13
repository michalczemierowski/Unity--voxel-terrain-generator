public struct BlockUpdateEventData
{
    public TerrainChunk chunk;
    public BlockPos position;
    public BlockType type;

    public BlockUpdateEventData(TerrainChunk chunk, BlockPos blockPos, BlockType blockType)
    {
        this.chunk = chunk;
        this.position = blockPos;
        this.type = blockType;
    }
}