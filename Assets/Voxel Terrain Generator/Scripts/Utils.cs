using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    [BurstCompile]
    public static class Utils
    {
        public static int ClampInRange(int value, int min, int max)
        {
            int result = value % max;
            return result < min ? max - math.abs(result) : result;
        }

        public static int BlockPosition3DtoIndex(BlockPosition pos)
        {
            return (pos.z * WorldSettings.fixedChunkWidth * WorldSettings.chunkHeight) + (pos.y * WorldSettings.fixedChunkWidth) + pos.x;
        }

        public static int BlockPosition3DtoIndex(int3 pos)
        {
            return (pos.z * WorldSettings.fixedChunkWidth * WorldSettings.chunkHeight) + (pos.y * WorldSettings.fixedChunkWidth) + pos.x;
        }

        public static int BlockPosition3DtoIndex(int x, int y, int z)
        {
            return (z * WorldSettings.fixedChunkWidth * WorldSettings.chunkHeight) + (y * WorldSettings.fixedChunkWidth) + x;
        }

        public static int BlockPosition2DtoIndex(int x, int z)
        {
            return x * WorldSettings.fixedChunkWidth + z;
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="x">local position x</param>
        /// <param name="y">local position y</param>
        /// <param name="z">local position z</param>
        /// <returns>World block position</returns>
        public static int3 LocalToWorldPositionInt3(Vector2Int cp, int x, int y, int z)
        {
            return new int3(x + cp.x - 1, y, z + cp.y - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static int3 LocalToWorldPositionInt3(Vector2Int cp, BlockPosition position)
        {
            return new int3(position.x + cp.x - 1, position.y, position.z + cp.y - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static Vector3Int LocalToWorldPositionVector3Int(Vector2Int cp, BlockPosition position)
        {
            return new Vector3Int(position.x + cp.x - 1, position.y, position.z + cp.y - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static int3 LocalToWorldPositionInt3(Vector2Int cp, int3 position)
        {
            return new int3(position.x + cp.x - 1, position.y, position.z + cp.y - 1);
        }
    }
}

