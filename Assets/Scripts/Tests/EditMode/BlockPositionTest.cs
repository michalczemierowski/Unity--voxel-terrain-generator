using NUnit.Framework;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Tests
{
    /// <summary>
    /// Check if block position clamp works correctly
    /// </summary>
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

        [Test]
        public void BlockPositionSpecificTest()
        {
            int x = WorldSettings.chunkWidth / 2;
            BlockPosition tested = new BlockPosition(WorldSettings.chunkWidth + x, 1, 1);

            Assert.AreEqual(tested.x, x + 1);
        }

        [Test]
        public void BlockPositionMethodsTest()
        {
            BlockPosition blockPosition = new BlockPosition() { x = 2, y = 2, z = 2 };

            BlockPosition above = blockPosition.Above();
            BlockPosition below = blockPosition.Below();

            BlockPosition added = blockPosition + new BlockPosition() { x = 1, y = 1, z = 1 };

            Assert.AreEqual(blockPosition + BlockPosition.up, above, "blockPosition.Above() != blockPosition + BlockPosition.up");
            Assert.AreEqual(blockPosition + BlockPosition.down, below, "blockPosition.Below() != blockPosition + BlockPosition.down");

            Assert.AreEqual(added, new BlockPosition(){ x = 3, y = 3, z = 3 }, "BlockPosition(2, 2, 2) + BlockPosition(1,1,1) != BlockPosition(3, 3, 3)");
        }
    }
}
