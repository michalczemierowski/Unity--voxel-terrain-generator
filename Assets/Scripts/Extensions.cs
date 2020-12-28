using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Entities.Items;
using VoxelTG.Player.Inventory;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using static VoxelTG.Terrain.WorldSettings;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Extensions
{
    public static class Extension
    {
        public static int ChunkIndexUp(this int index, int up)
        {
            return index + up * FixedChunkSizeX;
        }

        public static bool IsNullOrEmpty(this InventorySlot slot)
        {
            return slot == null || slot.IsEmpty();
        }
    }
}

