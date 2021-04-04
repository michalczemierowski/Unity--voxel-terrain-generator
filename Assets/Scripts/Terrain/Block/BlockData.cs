/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    public struct BlockData
    {
        public BlockType BlockType { get; private set; }
        public BlockPosition Position { get; private set; }
        public SetBlockSettings SetBlockSettings { get; private set; }

        public BlockData(BlockType blockType, BlockPosition position, SetBlockSettings setBlockSettings)
        {
            BlockType = blockType;
            Position = position;
            SetBlockSettings = setBlockSettings;
        }
    }
}