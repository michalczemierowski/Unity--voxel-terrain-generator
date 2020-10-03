using Unity.Mathematics;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public static class WorldSettings
    {
        public const int chunkWidth = 16;
        public const int chunkHeight = 128;
        public const int waterHeight = 60;
        public const int fixedChunkWidth = chunkWidth + 2;

        public const int maxTreeCount = 10;
        public const float minimumTreeDistance = 4;
        public static readonly int2 treeHeigthRange = new int2(4, 8);
        public const float chanceForGrass = 0.15f;

        public const int possibleBiomes = 2;
        public const float biomeSize = 3f;
    }
}
