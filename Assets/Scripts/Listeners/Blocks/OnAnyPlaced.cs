using System.Collections.Generic;
using UnityEngine;
using VoxelTG.Effects.VFX;
using VoxelTG.Listeners.Interfaces;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Blocks.Listeners
{
    public class OnAnyPlaced : MonoBehaviour, IBlockArrayPlaceListener
    {
        public BlockType[] GetBlockTypes()
        {
            // register this event listener to all blocks
            return Utils.GetAllBlockTypes();
        }

        public void OnBlockPlaced(BlockEventData data, params int[] args)
        {
            if (data.blockType == BlockType.AIR)
                return;

            World.ParticleManager.InstantiateBlockParticle(ParticleType.BLOCK_PLACE, data.WorldPosition, data.blockType, true);
        }
    }
}
