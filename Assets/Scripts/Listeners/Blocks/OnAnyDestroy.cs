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
    public class OnAnyDestroy : MonoBehaviour, IBlockArrayDestroyListener
    {   
        [Tooltip("Settings for sound that will be player when block is destroyed.")]
        [SerializeField] private Effects.SFX.SoundSettings destroySoundSettings = Effects.SFX.SoundSettings.DEFAULT;

        public BlockType[] GetBlockTypes()
        {
            // register this event listener to all blocks
            return Utils.GetAllBlockTypes();
        }

        public void OnBlockDestroy(BlockEventData data, params int[] args)
        {
            if (data.blockType == BlockType.AIR)
                return;

            BlockType type = data.blockType == BlockType.GRASS_BLOCK ? BlockType.DIRT : data.blockType;
            World.SoundManager.PlaySound(type, data.WorldPosition, destroySoundSettings);
            World.ParticleManager.InstantiateBlockParticle(ParticleType.BLOCK_DESTROY, data.WorldPosition, type, true);
        }
    }
}
