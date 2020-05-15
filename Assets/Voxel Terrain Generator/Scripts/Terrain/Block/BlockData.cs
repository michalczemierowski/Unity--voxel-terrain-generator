/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    public struct BlockData
    {
        public BlockType blockType;
        public BlockPosition position;

        public BlockData(BlockType blockType, BlockPosition position)
        {
            this.blockType = blockType;
            this.position = position;
        }
    }
}