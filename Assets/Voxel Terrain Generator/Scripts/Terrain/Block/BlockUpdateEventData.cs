using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    /// <summary>
    /// Contains data used in block event listeners
    /// </summary>
    public struct BlockEventData
    {
        public readonly Chunk chunk;
        public readonly BlockType blockType;

        public readonly BlockPosition LocalPosition;
        public readonly Vector3Int WorldPosition;

        public BlockEventData(Chunk chunk, BlockPosition localPosition, BlockType blockType)
        {
            this.chunk = chunk;
            this.LocalPosition = localPosition;
            this.blockType = blockType;

            this.WorldPosition = Utils.LocalToWorldPositionVector3Int(chunk.chunkPos, localPosition);
        }
    }
}