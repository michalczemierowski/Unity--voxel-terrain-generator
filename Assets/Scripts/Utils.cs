using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using static VoxelTG.Terrain.WorldSettings;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    [BurstCompile]
    public static class Utils
    {

        /// <summary>
        /// Custom clamp method used in BlockPosition
        /// ex. ClampInRange(40, 0, 16) will return 8 (40 - 16 - 16 = 8)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int ClampInRange(int value, int min, int max)
        {
            int result = value % max;
            return result < min ? max - math.abs(result) : result;
        }

        /// <summary>
        /// Convert 3D position to index
        /// </summary>
        /// <returns>index that can be used in Chunk.blocks</returns>
        public static int BlockPosition3DtoIndex(BlockPosition pos)
        {
            return (pos.z * FixedChunkSizeX * ChunkSizeY) + (pos.y * FixedChunkSizeX) + pos.x;
        }

        /// <summary>
        /// Convert 3D position to index
        /// </summary>
        /// <returns>index that can be used in Chunk.blocks</returns>
        public static int BlockPosition3DtoIndex(int3 pos)
        {
            return (pos.z * FixedChunkSizeX * ChunkSizeY) + (pos.y * FixedChunkSizeX) + pos.x;
        }

        /// <summary>
        /// Convert 3D position to index
        /// </summary>
        /// <returns>index that can be used in Chunk.blocks</returns>
        public static int BlockPosition3DtoIndex(int x, int y, int z)
        {
            return (z * FixedChunkSizeX * ChunkSizeY) + (y * FixedChunkSizeX) + x;
        }

        /// <summary>
        /// Convert 2D position to index
        /// </summary>
        /// <returns>index that can be used in Chunk.biomeTypes</returns>
        public static int BlockPosition2DtoIndex(int x, int z)
        {
            return x * FixedChunkSizeX + z;
        }

        /// <summary>
        /// Check if position is inside chunk bounds
        /// </summary>
        /// <returns>true if position is inside chunk bounds, else - false</returns>
        public static bool IsPositionInChunkBounds(int x, int y, int z)
        {
            return x > 0 && y >= 0 && z > 0 && x <= ChunkSizeXZ && z <= ChunkSizeXZ && y <= ChunkSizeY;
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
        /// <returns>Vector3Int world position</returns>
        public static Vector3Int LocalToWorldPositionVector3Int(Vector2Int cp, BlockPosition position)
        {
            return new Vector3Int(position.x + cp.x - 1, position.y, position.z + cp.y - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns>int3 world position</returns>
        public static int3 LocalToWorldPositionInt3(Vector2Int cp, int3 position)
        {
            return new int3(position.x + cp.x - 1, position.y, position.z + cp.y - 1);
        }

        /// <summary>
        /// Convert world position to block position
        /// </summary>
        /// <param name="worldPosition">world position</param>
        /// <returns>Vector3Int block position</returns>
        public static Vector3Int WorldToBlockPosition(Vector3 worldPosition)
        {
            return new Vector3Int(Mathf.FloorToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y), Mathf.FloorToInt(worldPosition.z));
        }

        /// <summary>
        /// Get all block types
        /// </summary>
        /// <returns>Array containing all possible block types</returns>
        public static BlockType[] GetAllBlockTypes()
        {
            return (BlockType[])System.Enum.GetValues(typeof(BlockType));
        }

        public static float RoundToDecimalPlace(float value, int place)
        {
            return math.round(value * (math.pow(10, place))) / math.pow(10, place);
        }
    }
}

