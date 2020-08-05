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
    public class OnDirtUpdate : MonoBehaviour, IBlockUpdateListener
    {
        [SerializeField] private GameObject destroyParticle;

        public BlockType GetBlockType()
        {
            return BlockType.DIRT;
        }

        private Mesh CreateParticleMesh(BlockType type, Mesh mesh)
        {
            Mesh cube = mesh;
            mesh.Clear();
            Block block = WorldData.GetBlock(type);

            mesh.vertices = new Vector3[]
            {
                new Vector3(0,0,0),
                new Vector3(0,1,0),
                new Vector3(1,1,0),
                new Vector3(1,0,0)
            };
            mesh.uv = new Vector2[]
            {
                block.sidePos.uv0,
                block.sidePos.uv1,
                block.sidePos.uv2,
                block.sidePos.uv3,
            };

            mesh.triangles = new int[]
            {
                0,
                1,
                2,
                0,
                2,
                3
            };

            cube.RecalculateNormals();
            return cube;
        }

        public void OnBlockUpdate(BlockEventData data, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args)
        {
            // if above block is not solid and there is grass block nearby - build grass block
            if (WorldData.GetBlockState(neighbours[BlockFace.TOP].blockType) != BlockState.SOLID && (
                neighbours[BlockFace.BACK].blockType == BlockType.GRASS_BLOCK ||
                neighbours[BlockFace.FRONT].blockType == BlockType.GRASS_BLOCK ||
                neighbours[BlockFace.LEFT].blockType == BlockType.GRASS_BLOCK ||
                neighbours[BlockFace.RIGHT].blockType == BlockType.GRASS_BLOCK))
            {
                // if args[0] == 1 build grass block
                if (args.Length > 0 && args[0] == 1)
                    data.chunk.AddBlockToBuildList(data.LocalPosition, BlockType.GRASS_BLOCK);
                else
                    // schedule grass build and pass args[0] = 1
                    World.ScheduleUpdate(data.chunk, data.LocalPosition, Random.Range(100, 200), 1);
            }
        }
    }
}
