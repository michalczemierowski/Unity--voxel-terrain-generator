using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//https://github.com/michalczemierowski
public class OnDirtUpdate : MonoBehaviour, IBlockUpdateListener
{
    public BlockType GetBlockType()
    {
        return BlockType.DIRT;
    }

    public void OnBlockUpdate(BlockUpdateEventData data, Dictionary<Side, BlockUpdateEventData> neighbours)
    {
        return;
        TerrainChunk chunk = data.chunk;
        Vector3 position = TerrainGenerator.LocalToWorldPositionVector3Int(chunk.chunkPos, data.position);
        if(Vector3.Distance(position, TerrainGenerator.player.transform.position) < 32)
        {
            chunk.AddBlockToBuildList(data.position, BlockType.STONE);
        }
        return;
        if(neighbours.Any(x => x.Value.type == BlockType.OBSIDIAN))
        {
            chunk.AddBlockToBuildList(data.position, BlockType.STONE);
        }
    }
}
