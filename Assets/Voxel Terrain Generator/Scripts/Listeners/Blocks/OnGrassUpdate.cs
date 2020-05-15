using System.Collections.Generic;
using UnityEngine;
using VoxelTG.Listeners.Interfaces;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Blocks.Listeners
{
    public class OnGrassUpdate : MonoBehaviour, IBlockArrayUpdateListener
    {
        BlockType[] IBlockArrayUpdateListener.GetBlockTypes()
        {
            List<BlockType> blocks = new List<BlockType>();
            foreach (BlockType type in System.Enum.GetValues(typeof(BlockType)))
            {
                if (WorldData.CanPlaceGrass(type))
                    blocks.Add(type);
            }
            return blocks.ToArray();
        }

        public void OnBlockUpdate(BlockUpdateEventData data, Dictionary<BlockFace, BlockUpdateEventData> neighbours)
        {
            //data.chunk.AddBlockToBuildList(data.position + BlockPosition.up * 2, BlockType.COBBLESTONE);
        }
    }
}