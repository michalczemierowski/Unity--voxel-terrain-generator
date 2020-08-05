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
            // register this event listener to all blocks
            return Utils.GetAllBlockTypes();
        }

        public void OnBlockDestroy(BlockEventData data, params int[] args)
        {
            BlockType type = data.blockType == BlockType.GRASS_BLOCK ? BlockType.DIRT : data.blockType;
            ParticleManager.InstantiateBlockDestroyParticle(ParticleType.BLOCK_DESTROY_PARTICLE, data.WorldPosition, type);
        }
    }
}
