using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Entities.Items;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using static VoxelTG.Terrain.WorldSettings;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    public static class Extensions
    {
        public static int ChunkIndexUp(this int index, int up)
        {
            return index + up * fixedChunkWidth;
        }
    }
}

