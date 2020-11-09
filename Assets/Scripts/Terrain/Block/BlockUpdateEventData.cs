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
        /// <summary>
        /// Chunk on which update happens
        /// </summary>
        public readonly Chunk chunk;
        /// <summary>
        /// Type of block that is calling update
        /// </summary>
        public readonly BlockType blockType;

        /// <summary>
        /// Local (chunk) space event position
        /// </summary>
        public readonly BlockPosition LocalPosition;
        /// <summary>
        /// World space event position
        /// </summary>
        public readonly Vector3Int WorldPosition;

        public BlockEventData(Chunk chunk, BlockPosition localPosition, BlockType blockType)
        {
            this.chunk = chunk;
            this.LocalPosition = localPosition;
            this.blockType = blockType;

            this.WorldPosition = Utils.LocalToWorldPositionVector3Int(chunk.ChunkPosition, localPosition);
        }
    }
}