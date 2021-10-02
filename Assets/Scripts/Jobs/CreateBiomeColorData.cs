using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTG.Terrain;
using static VoxelTG.WorldSettings;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Jobs
{
    [BurstCompile]
    public struct CreateBiomeColorData : IJob
    {
        [ReadOnly]
        public NativeArray<BiomeType> biomes;
        [ReadOnly]
        public NativeArray<Color> biomeColors;

        public NativeArray<Color32> colors;

        public void Execute()
        {
            // smoothing
            const int smoothDistanceHalf = 4;

            for (int x = 0; x < FixedChunkSizeXZ; x++)
            {
                for (int y = 0; y < FixedChunkSizeXZ; y++)
                {
                    int samples = 0;
                    float r = 0;
                    float g = 0;
                    float b = 0;
                    for (int xx = x - smoothDistanceHalf; xx < x + smoothDistanceHalf; xx++)
                    {
                        for (int yy = y - smoothDistanceHalf; yy < y + smoothDistanceHalf; yy++)
                        {
                            if (xx < 0 || xx >= FixedChunkSizeXZ || yy < 0 || yy >= FixedChunkSizeXZ)
                                continue;

                            Color sampleColor = GetBiomeColor(biomes[Utils.BlockPosition2DtoIndex(xx, yy)]);
                            r += sampleColor.r;
                            g += sampleColor.g;
                            b += sampleColor.b;
                            samples++;
                        }
                    }

                    colors[y * FixedChunkSizeXZ + x] = new Color(r / samples, g / samples, b / samples);
                }
            }
        }

        private Color GetBiomeColor(BiomeType biomeType)
        {
            int index = (int)biomeType;
            if (biomeColors.Length > index)
                return biomeColors[index];

            return Color.magenta;
        }
    }
}
