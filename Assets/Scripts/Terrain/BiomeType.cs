/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public enum BiomeType { FOREST, PLAINS, MOUNTAIN_PLAINS, MOUNTAINS, DESERT, COLD_PLAINS }

    public struct BiomeConfig
    {
        public BiomeType Type { get; set; }
        public float Height { get; set; }
        public float Temperature { get; set; }
        public float Moistrue { get; set; }
    }
}