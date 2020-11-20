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

        public BlockPosition(int3 position, bool clamp = true)
        {
            this.x = clamp ? Utils.ClampInRange(position.x + 1, 1, WorldSettings.ChunkSizeXZ) : position.x;
            this.y = clamp ? Utils.ClampInRange(position.y, 1, WorldSettings.ChunkSizeY) : position.y;
            this.z = clamp ? Utils.ClampInRange(position.z + 1, 1, WorldSettings.ChunkSizeXZ) : position.z;
        }

        public BlockPosition(Vector3Int position, bool clamp = true)
        {
            this.x = clamp ? Utils.ClampInRange(position.x + 1, 1, WorldSettings.ChunkSizeXZ) : position.x;
            this.y = clamp ? Utils.ClampInRange(position.y, 1, WorldSettings.ChunkSizeY) : position.y;
            this.z = clamp ? Utils.ClampInRange(position.z + 1, 1, WorldSettings.ChunkSizeXZ) : position.z;
        }

        public BlockPosition(int x, int y, int z, bool clamp = true)
        {
            this.x = clamp ? Utils.ClampInRange(x + 1, 1, WorldSettings.ChunkSizeXZ) : x;
            this.y = clamp ? Utils.ClampInRange(y, 1, WorldSettings.ChunkSizeY) : y;
            this.z = clamp ? Utils.ClampInRange(z + 1, 1, WorldSettings.ChunkSizeXZ) : z;
        }

        public BlockPosition(int x, int y, int z, out int neighbour)
        {
            neighbour = -1;
            if (x > WorldSettings.ChunkSizeXZ)
                neighbour = 0;
            else if (x < 1)
                neighbour = 1;
            if (z > WorldSettings.ChunkSizeXZ)
                neighbour = 2;
            else if (z < 1)
                neighbour = 3;

            this.x = Utils.ClampInRange(x, 1, WorldSettings.ChunkSizeXZ);
            this.y = Utils.ClampInRange(y, 1, WorldSettings.ChunkSizeY);
            this.z = Utils.ClampInRange(z, 1, WorldSettings.ChunkSizeXZ);
        }

        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(x, y, z);
        }
        public int3 ToInt3()
        {
            return new int3(x, y, z);
        }

        public BlockPosition RemoveOffset()
        {
            return new BlockPosition()
            {
                x = Utils.ClampInRange(x - 1, 1, WorldSettings.ChunkSizeXZ),
                y = this.y,
                z = Utils.ClampInRange(z - 1, 1, WorldSettings.ChunkSizeXZ)
            };
        }

        /// <summary>
        /// Get block position with Y reduced by 1
        /// </summary>
        public BlockPosition Below()
        {
            if (y < 1)
                return this;

            return new BlockPosition()
            {
                x = x,
                y = y - 1,
                z = z
            };
        }

        /// <summary>
        /// Get block position with Y increased by 1
        /// </summary>
        public BlockPosition Above()
        {
            if (y == WorldSettings.ChunkSizeY)
                return this;

            return new BlockPosition()
            {
                x = x,
                y = y + 1,
                z = z
            };
        }

        public void Add(int x, int y, int z, bool clamp = true)
        {
            this.x += x;
            this.y += y;
            this.z += z;
            if (clamp)
            {
                if (x != 0)
                    Utils.ClampInRange(x + 1, 1, WorldSettings.ChunkSizeXZ);
                if (y != 0)
                    Utils.ClampInRange(y, 1, WorldSettings.ChunkSizeY);
                if (z != 0)
                    Utils.ClampInRange(z + 1, 1, WorldSettings.ChunkSizeXZ);
            }
        }


        public override string ToString()
        {
            return "{" + x + ", " + y + ", " + z + "}";
        }

        public static BlockPosition operator +(BlockPosition bp1, BlockPosition bp2)
        {
            return new BlockPosition() { x = bp1.x + bp2.x, y = bp1.y + bp2.y, z = bp1.z + bp2.z };
        }
        public static BlockPosition operator -(BlockPosition bp1, BlockPosition bp2)
        {
            return new BlockPosition() { x = bp1.x - bp2.x, y = bp1.y - bp2.y, z = bp1.z - bp2.z };
        }
        public static BlockPosition operator *(BlockPosition bp1, int m)
        {
            return new BlockPosition() { x = bp1.x * m, y = bp1.y * m, z = bp1.z * m };
        }

        /// <summary>
        /// [x: 0, y: 1, z: 0]
        /// </summary>
        public static readonly BlockPosition up = new BlockPosition() { x = 0, y = 1, z = 0 };
        /// <summary>
        /// [x: 0, y: -1, z: 0]
        /// </summary>
        public static readonly BlockPosition down = new BlockPosition() { x = 0, y = -1, z = 0 };
        /// <summary>
        /// [x: -1, y: 0, z: 0]
        /// </summary>
        public static readonly BlockPosition left = new BlockPosition() { x = -1, y = 0, z = 0 };
        /// <summary>
        /// [x: 1, y: 0, z: 0]
        /// </summary>
        public static readonly BlockPosition right = new BlockPosition() { x = 1, y = 0, z = 0 };
    }
}
