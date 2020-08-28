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
    public class OnStonePlaced : MonoBehaviour, IBlockPlaceListener
    {
        public BlockType GetBlockType()
        {
            return BlockType.STONE;
        }

        public void OnBlockPlaced(BlockEventData data, params int[] args)
        {
            Debug.Log("ON STONE PLACED");
        }
    }
}
