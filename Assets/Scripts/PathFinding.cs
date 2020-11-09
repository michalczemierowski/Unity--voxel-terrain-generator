using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
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
    public static class PathFinding
    {
        public const int SINGLE_AXIS_COST = 10;
        public const int DOUBLE_AXIS_COST = 14;
        public const int TRIPLE_AXIS_COST = 17;

        private static int entityHeight;
        private static int maxChecks;

        private static int3[] possiblePositions = new int3[24];

        public static int MaxPathChecks => maxChecks;

        public static void Init()
        {
            maxChecks = 64;
        }

        public static void Dispose()
        {

        }

        // TODO: cleanup
        public static Vector3[] FindPath(Vector3Int startPosition, Vector3Int targetPosition, int entityHeight = 2)
        {
            int3 startPos = new int3((int)math.round(startPosition.x), (int)math.round(startPosition.y), (int)math.round(startPosition.z));
            int3 targetPos = new int3((int)math.round(targetPosition.x), (int)math.round(targetPosition.y), (int)math.round(targetPosition.z));

            PathFinding.entityHeight = entityHeight;

            PathTile startTile = new PathTile()
            {
                Position = startPos
            };
            PathTile targetTile = new PathTile()
            {
                Position = targetPos
            };

            startTile.CalculateCost(targetPos);

            List<PathTile> activeTiles = new List<PathTile>(32);
            List<PathTile> visitedTiles = new List<PathTile>(32);
            activeTiles.Add(startTile);

            int checks = 0;
            while (activeTiles.Any())
            {
                if (checks >= MaxPathChecks)
                    return null;
                PathTile checkTile = activeTiles.OrderBy(x => x.Cost).First();
                if (checkTile.Position.Equals(targetTile.Position))
                {
                    Debug.Log("found path");
                    List<Vector3> path = new List<Vector3>();

                    PathTile lastTile = checkTile;
                    while (lastTile != null)
                    {
                        path.Add(new Vector3(lastTile.Position.x + 0.5f, lastTile.Position.y, lastTile.Position.z + 0.5f));
                        lastTile = lastTile.CameFrom;
                    }

                    path.Reverse();

                    //World.Instance.lineRenderer.positionCount = path.Count;
                   // World.Instance.lineRenderer.SetPositions(path.ToArray());
                    return path.ToArray();
                }

                visitedTiles.Add(checkTile);
                activeTiles.Remove(checkTile);

                List<PathTile> walkableTiles = GetWalkableTiles(checkTile, targetTile);
                foreach (PathTile tile in walkableTiles)
                {
                    if (visitedTiles.Any((x) => x.Position.Equals(tile.Position)))
                        continue;

                    if (activeTiles.Any((x) => x.Position.Equals(tile.Position)))
                    {
                        PathTile existing = activeTiles.First((x) => x.Position.Equals(tile.Position));
                        if (existing.Cost > checkTile.Cost)
                        {
                            activeTiles.Remove(existing);
                            activeTiles.Add(checkTile);
                        }
                    }
                    else
                    {
                        activeTiles.Add(tile);
                    }
                }
                checks++;
            }

            Debug.Log("NO PATH FOUND");
            return new Vector3[0];
        }

        private static List<PathTile> GetWalkableTiles(PathTile currentTile, PathTile targetTile)
        {
            int3 currentPos = currentTile.Position;

            /// ===== UP ===== ///
            possiblePositions[0] = new int3(currentPos.x + 1, currentPos.y + 1, currentPos.z);         // RIGHT
            possiblePositions[1] = new int3(currentPos.x - 1, currentPos.y + 1, currentPos.z);         // LEFT
            possiblePositions[2] = new int3(currentPos.x, currentPos.y + 1, currentPos.z + 1);         // FRONT
            possiblePositions[3] = new int3(currentPos.x, currentPos.y + 1, currentPos.z - 1);         // BACK
            possiblePositions[4] = new int3(currentPos.x + 1, currentPos.y + 1, currentPos.z + 1);     // RIGHT FRONT
            possiblePositions[5] = new int3(currentPos.x - 1, currentPos.y + 1, currentPos.z + 1);     // LEFT FRONT
            possiblePositions[6] = new int3(currentPos.x + 1, currentPos.y + 1, currentPos.z - 1);     // RIGHT BACK
            possiblePositions[7] = new int3(currentPos.x - 1, currentPos.y + 1, currentPos.z - 1);     // LEFT BACK

            /// ===== CENTER ===== ///
            possiblePositions[8] = new int3(currentPos.x + 1, currentPos.y, currentPos.z);             // RIGHT
            possiblePositions[9] = new int3(currentPos.x - 1, currentPos.y, currentPos.z);             // LEFT
            possiblePositions[10] = new int3(currentPos.x, currentPos.y, currentPos.z + 1);             // FRONT
            possiblePositions[11] = new int3(currentPos.x, currentPos.y, currentPos.z - 1);             // BACK
            possiblePositions[12] = new int3(currentPos.x + 1, currentPos.y, currentPos.z + 1);         // RIGHT FRONT
            possiblePositions[13] = new int3(currentPos.x - 1, currentPos.y, currentPos.z + 1);         // LEFT FRONT
            possiblePositions[14] = new int3(currentPos.x + 1, currentPos.y, currentPos.z - 1);         // RIGHT BACK
            possiblePositions[15] = new int3(currentPos.x - 1, currentPos.y, currentPos.z - 1);         // LEFT BACK

            /// ===== DOWN ===== ///
            possiblePositions[16] = new int3(currentPos.x + 1, currentPos.y - 1, currentPos.z);         // RIGHT
            possiblePositions[17] = new int3(currentPos.x - 1, currentPos.y - 1, currentPos.z);         // LEFT
            possiblePositions[18] = new int3(currentPos.x, currentPos.y - 1, currentPos.z + 1);         // FRONT
            possiblePositions[19] = new int3(currentPos.x, currentPos.y - 1, currentPos.z - 1);         // BACK
            possiblePositions[20] = new int3(currentPos.x + 1, currentPos.y - 1, currentPos.z + 1);     // RIGHT FRONT
            possiblePositions[21] = new int3(currentPos.x - 1, currentPos.y - 1, currentPos.z + 1);     // LEFT FRONT
            possiblePositions[22] = new int3(currentPos.x + 1, currentPos.y - 1, currentPos.z - 1);     // RIGHT BACK
            possiblePositions[23] = new int3(currentPos.x - 1, currentPos.y - 1, currentPos.z - 1);     // LEFT BACK

            List<PathTile> pathTiles = new List<PathTile>();
            foreach (int3 position in possiblePositions)
            {
                if (World.TryGetChunk((float)position.x, (float)position.z, out Chunk chunk))
                {
                    int3 releativePosition = new BlockPosition(position).ToInt3();//new int3(position.x - chunk.ChunkPosition.x, position.y, position.z - chunk.ChunkPosition.y);
                    if (IsInRange(ref releativePosition) && CanWalkOn(ref chunk.blocks, ref releativePosition))
                    {
                        PathTile pathTile = new PathTile()
                        {
                            Position = position,
                            CameFrom = currentTile,
                        };
                        pathTile.CalculateCost(targetTile.Position);

                        pathTiles.Add(pathTile);
                    }
                }
            }

            return pathTiles;
        }

        private static bool IsInRange(ref int3 pos)
        {
            return pos.x > 0
                && pos.y >= 0
                && pos.z > 0
                && pos.x <= chunkWidth
                && pos.z <= chunkWidth
                && pos.y <= chunkHeight;
        }

        private static bool CanWalkOn(ref NativeArray<BlockType> blocks, ref int3 pos)
        {
            int index = Utils.BlockPosition3DtoIndex(pos.x, pos.y, pos.z);
            bool walkableBelow = WorldData.GetBlockState(blocks[index.ChunkIndexUp(-1)]) == BlockState.SOLID;
            bool walkableAt = (blocks[index] == BlockType.AIR || blocks[index] == BlockType.GRASS);

            bool walkableAbove = true;
            for (int y = 1; y < entityHeight; y++)
            {
                if (blocks[index.ChunkIndexUp(y)] != BlockType.AIR)
                {
                    walkableAbove = false;
                    break;
                }
            }

            return walkableBelow
                && walkableAt
                && walkableAbove;
        }

        private static int CalculateCost(int3 pos, int3 targetPosition)
        {
            int xDistance = math.abs(targetPosition.x - pos.x);
            int yDistance = math.abs(targetPosition.y - pos.y);
            int zDistance = math.abs(targetPosition.z - pos.z);

            int minimum = math.min(math.min(xDistance, yDistance), zDistance);
            int maximum = math.max(math.max(xDistance, yDistance), zDistance);

            int tripleAxis = minimum;
            int doubleAxis = math.max(xDistance + yDistance + zDistance - maximum - 2 * minimum, 0);
            int singleAxis = maximum - doubleAxis - tripleAxis;

            return SINGLE_AXIS_COST * singleAxis
                + DOUBLE_AXIS_COST * doubleAxis
                + TRIPLE_AXIS_COST * tripleAxis;
        }
    }

    public class PathTile
    {
        public int3 Position { get; set; }
        public int Cost { get; set; }
        public PathTile CameFrom { get; set; }

        public void CalculateCost(int3 targetPosition)
        {
            int3 distance = math.abs(targetPosition - Position);

            int minimum = math.min(math.min(distance.x, distance.y), distance.z);
            int maximum = math.max(math.max(distance.x, distance.y), distance.z);

            int tripleAxis = minimum;
            int doubleAxis = math.max(distance.x + distance.y + distance.z - maximum - 2 * minimum, 0);
            int singleAxis = maximum - doubleAxis - tripleAxis;

            Cost = PathFinding.SINGLE_AXIS_COST * singleAxis
                + PathFinding.DOUBLE_AXIS_COST * doubleAxis
                + PathFinding.TRIPLE_AXIS_COST * tripleAxis;
        }
    }
}
