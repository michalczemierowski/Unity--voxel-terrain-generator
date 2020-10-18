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
        public bool callUpdate;

        public BlockData(BlockType blockType, BlockPosition position, bool callUpdate = true)
        {
            this.blockType = blockType;
            this.position = position;
            this.callUpdate = callUpdate;
        }
    }
}