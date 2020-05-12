public struct BlockData
{
    public BlockType blockType;
    public BlockPos position;

    public BlockData(BlockType blockType, BlockPos position)
    {
        this.blockType = blockType;
        this.position = position;
    }
}
