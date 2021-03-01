using Unity.Collections;
using Unity.Mathematics;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    // TODO: xml comments
    public static class WorldSettings
    {
        public static void Init()
        {
            Biomes.Init();
        }

        public static void Dispose()
        {
            Biomes.Dispose();
        }

        public const int ChunkSizeXZ = 16;
        public const int ChunkSizeY = 256;
        public const int WaterLevelY = 50;
        public const float BaseLandHeightMultipler = 0.2f;
        public const int FixedChunkSizeXZ = ChunkSizeXZ + 2;

        public const int MaxTreesPerChunk = 10;
        public const float MinDistanceBetweenTrees = 4;
        public static readonly int2 TreeHeightRange = new int2(4, 8);
        public const float ChanceForGrass = 0.15f;

        public static class Biomes
        {
            public const float BiomeSize = 5f;

            public static NativeArray<BiomeConfig> biomeConfigs { get; private set; }

            public static void Init()
            {
                biomeConfigs = new NativeArray<BiomeConfig>(new BiomeConfig[]
                {
                    new BiomeConfig
                    {
                        Type = BiomeType.FOREST,
                        Temperature = 1,
                        Height = 0.3f,
                        Moistrue = 0.5f
                    },
                    new BiomeConfig
                    {
                        Type = BiomeType.PLAINS,
                        Temperature = 0.95f,
                        Height = 0.3f,
                        Moistrue = 0.5f
                    },
                    new BiomeConfig
                    {
                        Type = BiomeType.MOUNTAIN_PLAINS,
                        Temperature = 0.35f,
                        Height = 1.35f,
                        Moistrue = 0.2f
                    },
                    new BiomeConfig
                    {
                        Type = BiomeType.MOUNTAINS,
                        Temperature = 0.3f,
                        Height = 1.6f,
                        Moistrue = 0.2f
                    },
                    new BiomeConfig
                    {
                        Type = BiomeType.DESERT,
                        Temperature = 1.6f,
                        Height = 1f,
                        Moistrue = 0.1f
                    },
                    new BiomeConfig
                    {
                        Type = BiomeType.COLD_PLAINS,
                        Temperature = 0.3f,
                        Height = 0.95f,
                        Moistrue = 0.5f
                    }
                }, Allocator.Persistent);
            }

            public static void Dispose()
            {
                biomeConfigs.Dispose();
            }

            public static BiomeType GetBiome(int x, int z)
            {
                float height = World.FastNoise.GetSimplex(x / BiomeSize, z / BiomeSize) + 1;
                float temperature = World.FastNoise.GetSimplex((x / BiomeSize) * 0.1f, (z / BiomeSize) * 0.1f) + 1;
                float moistrue = World.FastNoise.GetSimplex((x / BiomeSize) * 0.25f, (z / BiomeSize) * 0.25f) + 1;

                BiomeType selectedType = BiomeType.PLAINS;
                float minDiff = float.MaxValue;
                for (int i = 0; i < biomeConfigs.Length; i++)
                {
                    BiomeConfig config = biomeConfigs[i];
                    float diff = math.abs(config.Height - height)
                             + math.abs(config.Temperature - temperature)
                             + math.abs(config.Moistrue - moistrue);

                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        selectedType = config.Type;
                    }
                }

                return selectedType;
            }
        }

        public static class Trees
        {
            public static readonly TreeSettings OakTree = new TreeSettings() { TreeHeightRange = new int2(5, 12) };
        }

        public static class Textures
        {
            public const float BlockTextureSize = 16f;

            /// <summary>
            /// Name of game object whose mesh will be replaced with block cube when loading hand object for Material item.
            /// </summary>
            public static readonly string BlockCubeName = "block_cube";
        }
    }

    public struct TreeSettings
    {
        public int2 TreeHeightRange;
    }
}
