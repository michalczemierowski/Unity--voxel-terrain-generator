using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace Game.PathFinding
{
    public static class PathFinding
    {
        // TODO
        public static Vector3Int[] FindPath(Vector3Int startPosition, Vector3Int targetPosition)
        {
            Chunk chunk = World.GetChunk(startPosition.x, startPosition.z);

            int3 startPos = new BlockPosition(Vector3Int.RoundToInt(startPosition)).ToInt3();
            int3 targetPos = new BlockPosition(Vector3Int.RoundToInt(targetPosition)).ToInt3();

            NativeList<Vector3Int> jobResult = new NativeList<Vector3Int>(64, Allocator.TempJob);

            FindPathJob findPath = new FindPathJob()
            {
                blocks = chunk.blocks,
                startPosition = startPos,
                targetPosition = targetPos,

                result = jobResult
            };

            JobHandle handle = findPath.Schedule();

            handle.Complete();

            Vector3Int[] result = jobResult.ToArray();

            jobResult.Dispose();

            return result;
        }

    }

    public struct FindPathJob : IJob
    {
        [ReadOnly]
        public NativeArray<BlockType> blocks;

        public int3 startPosition;
        public int3 targetPosition;

        public NativeList<Vector3Int> result;

        public void Execute()
        {
            result.Add(new Vector3Int(startPosition.x, startPosition.y, startPosition.z));
            result.Add(new Vector3Int(targetPosition.x, targetPosition.y, targetPosition.z));

            // TODO: pathfinding
        }
    }
}
