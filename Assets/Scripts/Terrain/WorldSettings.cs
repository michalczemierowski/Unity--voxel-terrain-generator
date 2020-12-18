using Unity.Mathematics;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    // TODO: xml comments
    public static class WorldSettings
    {
        public const int ChunkSizeXZ = 16;
        public const int ChunkSizeY = 128;
        public const int WaterLevelY = 58;
        public const int FixedChunkSizeX = ChunkSizeXZ + 2;

        public const int MaxTreesPerChunk = 10;
        public const float MinDistanceBetweenTrees = 4;
        public static readonly int2 TreeHeightRange = new int2(4, 8);
        public const float ChanceForGrass = 0.15f;

        public const int PossibleBiomes = 4;
        public const float BiomeSize = 8f;
        public const float BiomeHeightMultipler = 4f;
        public const float BiomeTransition = 0.005f;
    }
}
