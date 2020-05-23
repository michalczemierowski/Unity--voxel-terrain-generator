using System.Collections.Generic;
using UnityEngine;
using VoxelTG.Effects;
using VoxelTG.Listeners.Interfaces;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Blocks.Listeners
{
    public class OnAnyDestroy : MonoBehaviour, IBlockArrayDestroyListener
    {
        public BlockType[] GetBlockTypes()
        {
            return (BlockType[])System.Enum.GetValues(typeof(BlockType));
        }

        public void OnBlockDestroy(BlockEventData data, params int[] args)
        {
            BlockType type = data.type == BlockType.GRASS_BLOCK ? BlockType.DIRT : data.type;
            ParticleManager.InstantiateBlockParticle(type, World.LocalToWorldPositionVector3Int(data.chunk.chunkPos, data.position));
        }
    }
}
