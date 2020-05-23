/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    public struct BlockEventData
    {
        public Chunk chunk;
        public BlockPosition position;
        public BlockType type;

        public BlockEventData(Chunk chunk, BlockPosition blockPos, BlockType blockType)
        {
            this.chunk = chunk;
            this.position = blockPos;
            this.type = blockType;
        }
    }
}