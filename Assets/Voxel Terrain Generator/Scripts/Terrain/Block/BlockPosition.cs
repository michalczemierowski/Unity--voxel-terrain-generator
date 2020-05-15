using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    public struct BlockPosition
    {
        public int x, y, z;

        public BlockPosition(int x, int y, int z)
        {
            this.x = Utils.ClampInRange(x, 1, Chunk.chunkWidth);
            this.y = Utils.ClampInRange(y, 1, Chunk.chunkHeight);
            this.z = Utils.ClampInRange(z, 1, Chunk.chunkWidth);
        }

        public BlockPosition(int x, int y, int z, out int neighbour)
        {
            neighbour = -1;
            if (x > Chunk.chunkWidth)
                neighbour = 0;
            else if (x < 1)
                neighbour = 1;
            if (z > Chunk.chunkWidth)
                neighbour = 2;
            else if (z < 1)
                neighbour = 3;

            this.x = Utils.ClampInRange(x, 1, Chunk.chunkWidth);
            this.y = Utils.ClampInRange(y, 1, Chunk.chunkHeight);
            this.z = Utils.ClampInRange(z, 1, Chunk.chunkWidth);
        }

        public void Add(int x, int y, int z)
        {
            if (x != 0)
            {
                this.x += x;
                if (this.x > Chunk.chunkWidth)
                    this.x -= Chunk.chunkWidth;
                else if (x < 1)
                    this.x += Chunk.chunkWidth;
            }
            if (y != 0)
            {
                this.y += y;
                if (this.y > Chunk.chunkWidth)
                    this.y -= Chunk.chunkWidth;
                else if (y < 1)
                    this.y += Chunk.chunkWidth;
            }
            if (z != 0)
            {
                this.z += z;
                if (this.z > Chunk.chunkWidth)
                    this.z -= Chunk.chunkWidth;
                else if (z < 1)
                    this.z += Chunk.chunkWidth;
            }
        }

        public static BlockPosition WorldToBlockPosition(float x, float y, float z)
        {
            while (x > Chunk.chunkWidth)
                x -= Chunk.chunkWidth;
            while (x < 1)
                x += Chunk.chunkWidth;
            while (z > Chunk.chunkWidth)
                z -= Chunk.chunkWidth;
            while (z < 1)
                z += Chunk.chunkWidth;

            return new BlockPosition(Mathf.FloorToInt(x) + 1, Mathf.FloorToInt(y), Mathf.FloorToInt(z) + 1);
        }

        public override string ToString()
        {
            return "{" + x + ", " + y + ", " + z + "}";
        }

        public static BlockPosition operator +(BlockPosition bp1, BlockPosition bp2)
        {
            return new BlockPosition(bp1.x + bp2.x, bp1.y + bp2.y, bp1.z + bp2.z);
        }
        public static BlockPosition operator -(BlockPosition bp1, BlockPosition bp2)
        {
            return new BlockPosition(bp1.x - bp2.x, bp1.y - bp2.y, bp1.z - bp2.z);
        }
        public static BlockPosition operator *(BlockPosition bp1, int m)
        {
            return new BlockPosition(bp1.x * m, bp1.y * m, bp1.z * m);
        }

        public static readonly BlockPosition up = new BlockPosition(0, 1, 0);
        public static readonly BlockPosition down = new BlockPosition(0, -1, 0);
        public static readonly BlockPosition left = new BlockPosition(0, -1, 0);
        public static readonly BlockPosition right = new BlockPosition(0, 1, 0);
    }
}
