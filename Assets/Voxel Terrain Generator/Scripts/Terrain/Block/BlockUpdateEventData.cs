/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    public struct BlockUpdateEventData
    {
        public Chunk chunk;
        public BlockPosition position;
        public BlockType type;

        public BlockUpdateEventData(Chunk chunk, BlockPosition blockPos, BlockType blockType)
        {
            this.chunk = chunk;
            this.position = blockPos;
            this.type = blockType;
        }
    }
}