using NUnit.Framework;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

namespace VoxelTG.Tests
{
    public class BlockPositionTest
    {
        [Test]
        public void BlockPositionRangeTest()
        {
            BlockPosition bp;
            for (int x = 0; x < WorldSettings.fixedChunkWidth * 2; x++)
            {
                bp = new BlockPosition(x, 0, 0);
                Assert.IsTrue(bp.x >= 1 && bp.x <= WorldSettings.chunkWidth);
            }
            for (int y = 0; y < WorldSettings.chunkHeight * 2; y++)
            {
                bp = new BlockPosition(0, y, 0);
                Assert.IsTrue(bp.y >= 0 && bp.y <= WorldSettings.chunkHeight);
            }
            for (int z = 0; z < WorldSettings.fixedChunkWidth * 2; z++)
            {
                bp = new BlockPosition(0, 0, z);
                Assert.IsTrue(bp.z >= 1 && bp.z <= WorldSettings.chunkWidth);
            }
        }
    }
}
