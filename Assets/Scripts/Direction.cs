using Unity.Mathematics;
using UnityEngine;

namespace VoxelTG
{
    public enum Direction
    {
        N,
        S,
        W,
        E,
        NW,
        NE,
        SW,
        SE
    }

    public static class DriectionMethods
    {
        public static int2 ToInt2(this Direction direction)
        {
            switch(direction)
            {
                case Direction.N:
                    return new int2(0, 1);
                case Direction.S:
                    return new int2(0, -1);
                case Direction.W:
                    return new int2(-1, 0);
                case Direction.E:
                    return new int2(1, 0);
                case Direction.NW:
                    return new int2(-1, 1);
                case Direction.NE:
                    return new int2(1, 1);
                case Direction.SW:
                    return new int2(-1, -1);
                case Direction.SE:
                    return new int2(1, -1);
                default:
                    return int2.zero;
            }
        }

        public static Vector2Int ToVector2Int(this Direction direction)
        {
            int2 dir = direction.ToInt2();
            return new Vector2Int(dir.x, dir.y);
        }

        public static Direction GetOpposite(this Direction direction)
        {
            switch (direction)
            {
                case Direction.N:
                    return Direction.S;
                case Direction.S:
                    return Direction.N;
                case Direction.W:
                    return Direction.E;
                case Direction.E:
                    return Direction.W;
                case Direction.NW:
                    return Direction.SE;
                case Direction.NE:
                    return Direction.SW;
                case Direction.SW:
                    return Direction.NE;
                case Direction.SE:
                    return Direction.NW;
                default:
                    return Direction.N;
            }
        }
    }
}