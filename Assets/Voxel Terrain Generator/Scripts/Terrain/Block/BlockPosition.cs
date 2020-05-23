using Unity.Mathematics;
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

        public BlockPosition(int x, int y, int z, bool clamp = true)
        {
            this.x = clamp ? Utils.ClampInRange(x, 1, WorldSettings.chunkWidth) : x;
            this.y = clamp ? Utils.ClampInRange(y, 1, WorldSettings.chunkHeight) : y;
            this.z = clamp ? Utils.ClampInRange(z, 1, WorldSettings.chunkWidth) : z;
        }

        public BlockPosition(int x, int y, int z, out int neighbour)
        {
            neighbour = -1;
            if (x > WorldSettings.chunkWidth)
                neighbour = 0;
            else if (x < 1)
                neighbour = 1;
            if (z > WorldSettings.chunkWidth)
                neighbour = 2;
            else if (z < 1)
                neighbour = 3;

            this.x = Utils.ClampInRange(x, 1, WorldSettings.chunkWidth);
            this.y = Utils.ClampInRange(y, 1, WorldSettings.chunkHeight);
            this.z = Utils.ClampInRange(z, 1, WorldSettings.chunkWidth);
        }

        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(x, y, z);
        }
        public int3 ToInt3()
        {
            return new int3(x, y, z);
        }

        public BlockPosition Below()
        {
            if (y < 1)
                return this;
            
            return new BlockPosition(x, y - 1, z, false);
        }
        public BlockPosition Above()
        {
            if (y == WorldSettings.chunkHeight)
                return this;

            return new BlockPosition(x, y + 1, z, false);
        }

        public void Add(int x, int y, int z)
        {
            if (x != 0)
            {
                this.x += x;
                if (this.x > WorldSettings.chunkWidth)
                    this.x -= WorldSettings.chunkWidth;
                else if (x < 1)
                    this.x += WorldSettings.chunkWidth;
            }
            if (y != 0)
            {
                this.y += y;
                if (this.y > WorldSettings.chunkWidth)
                    this.y -= WorldSettings.chunkWidth;
                else if (y < 1)
                    this.y += WorldSettings.chunkWidth;
            }
            if (z != 0)
            {
                this.z += z;
                if (this.z > WorldSettings.chunkWidth)
                    this.z -= WorldSettings.chunkWidth;
                else if (z < 1)
                    this.z += WorldSettings.chunkWidth;
            }
        }

        public static BlockPosition WorldSettingsToBlockPosition(float x, float y, float z)
        {
            while (x > WorldSettings.chunkWidth)
                x -= WorldSettings.chunkWidth;
            while (x < 1)
                x += WorldSettings.chunkWidth;
            while (z > WorldSettings.chunkWidth)
                z -= WorldSettings.chunkWidth;
            while (z < 1)
                z += WorldSettings.chunkWidth;

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
