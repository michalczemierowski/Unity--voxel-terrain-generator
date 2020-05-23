using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Effects
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance;

        [SerializeField] private GameObject onBlockDestroyParticle;
        private float onBDPDestroyTime = 1; // onBlockDestroyParticle destroy time

        private void Awake()
        {
            if (Instance)
                Destroy(this);
            else
                Instance = this;
        }

        public static void InstantiateBlockParticle(BlockType type, Vector3Int blockPosition)
        {
            ParticleSystem particle = Instantiate(Instance.onBlockDestroyParticle, new Vector3(blockPosition.x + 0.5f,
                blockPosition.y, blockPosition.z + 0.5f), Quaternion.identity).GetComponent<ParticleSystem>();

            ParticleSystemRenderer particleSystemRenderer = particle.GetComponent<ParticleSystemRenderer>();
            particleSystemRenderer.mesh = CreateBlockParticleMeh(type, particleSystemRenderer.mesh);

            particle.Play();
            Destroy(particle.gameObject, Instance.onBDPDestroyTime);
        }

        private static Mesh CreateBlockParticleMeh(BlockType type, Mesh mesh)
        {
            Mesh particleMesh = mesh;
            mesh.Clear();
            Block block = WorldData.GetBlock(type);

            particleMesh.vertices = new Vector3[]
            {
                new Vector3(0,0,0),
                new Vector3(0,1,0),
                new Vector3(1,1,0),
                new Vector3(1,0,0)
            };
            float quater = 0.25f / Terrain.Blocks.TilePos.textureSize;
            particleMesh.uv = new Vector2[]
            {
                block.sidePos.uv0 + new float2(quater, quater),
                block.sidePos.uv1 + new float2(quater, -quater),
                block.sidePos.uv2 + new float2(-quater, -quater),
                block.sidePos.uv3 + new float2(-quater, quater),
            };
            particleMesh.triangles = new int[]
            {
                0,
                1,
                2,
                0,
                2,
                3
            };

            particleMesh.RecalculateNormals();
            return particleMesh;
        }
    }
}
