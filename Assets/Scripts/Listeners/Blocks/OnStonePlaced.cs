using System.Collections.Generic;
using UnityEngine;
using VoxelTG.Listeners.Interfaces;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
using VoxelTG;
using VoxelTG.Entities;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Blocks.Listeners
{
    public class OnStonePlaced : MonoBehaviour, IBlockArrayPlaceListener
    {
        private Vector3Int posOne;
        private Vector3Int posTwo;

        public BlockType[] GetBlockTypes()
        {
            return new BlockType[] { BlockType.OBSIDIAN, BlockType.STONE };
        }

        public void OnBlockPlaced(BlockEventData data, params int[] args)
        {
            if (data.blockType == BlockType.STONE)
                posOne = data.WorldPosition + Vector3Int.up;
            else
                posTwo = data.WorldPosition + Vector3Int.up;



            // BlockPosition blockPosition = World.GetTopSolidBlock(new Vector2Int(data.WorldPosition.x, data.WorldPosition.z), out Chunk chunk);
            // blockPosition.y += 2;
            // chunk.SetBlock(blockPosition, BlockType.OAK_LEAVES, SetBlockSettings.PLACE);
        }
    }
}
