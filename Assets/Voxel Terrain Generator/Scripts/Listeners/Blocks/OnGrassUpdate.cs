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
    public class OnGrassUpdate : MonoBehaviour, IBlockArrayUpdateListener, IBlockArrayDestroyListener
    {
        public BlockType[] GetBlockTypes()
        {
            List<BlockType> blocks = new List<BlockType>();
            foreach (BlockType type in System.Enum.GetValues(typeof(BlockType)))
            {
                if (WorldData.CanPlaceGrass(type))
                    blocks.Add(type);
            }
            return blocks.ToArray();
        }

        public void OnBlockUpdate(BlockEventData data, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args)
        {
            //data.chunk.AddBlockToBuildList(data.position + BlockPosition.up * 2, BlockType.COBBLESTONE);
        }

        public void OnBlockDestroy(BlockEventData data, params int[] args)
        {
            BlockPosition above = data.position.Above();
            if (data.chunk.GetBlock(above) == BlockType.GRASS)
            {
                data.chunk.AddBlockToBuildList(new BlockData(BlockType.AIR, above));
                ParticleManager.InstantiateBlockDestroyParticle(ParticleType.BLOCK_DESTROY_PARTICLE, Utils.LocalToWorldPositionVector3Int(data.chunk.chunkPos, above), BlockType.GRASS);
            }
        }
    }
}