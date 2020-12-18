using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Effects.VFX
{
    public class ParticleManager : MonoBehaviour
    {
        [Tooltip("Particle that will be instantiated when block is destroyed")]
        [SerializeField] private GameObject onBlockDestroyParticle;
        [Tooltip("Particle that will be instantiated when block is placed")]
        [SerializeField] private GameObject onBlockPlaceParticle;
        [Tooltip("Particle that will be instantiated when bullet hits block")]
        [SerializeField] private GameObject onBulletHitParticle;

        /// <summary>
        /// Particles mesh cache
        /// </summary>
        private static Dictionary<BlockType, Mesh> particleMeshes = new Dictionary<BlockType, Mesh>();

        /// <summary>
        /// Max size of particle pool
        /// </summary>
        private const int targetPoolSize = 32;
        /// <summary>
        /// Particles GameObjects pool
        /// </summary>
        private static Dictionary<ParticleType, Queue<ParticleSystem>> particlesPool = new Dictionary<ParticleType, Queue<ParticleSystem>>();

        /// <summary>
        /// Size of particle texture UV (range 0 - 1)
        /// </summary>
        private static float blockDestroyParticleUvSize = 0.25f;


        private void Start()
        {
            // init queue for particle of each type
            foreach (ParticleType particleType in System.Enum.GetValues(typeof(ParticleType)))
            {
                particlesPool.Add(particleType, new Queue<ParticleSystem>());
            }

            StartCoroutine(BlockParticlesPoolCleaner(0.5f));
        }

        /// <summary>
        /// Instantiate block particle of type <see cref="ParticleType"/> at specified world position
        /// </summary>
        /// <param name="particleType"></param>
        /// <param name="worldPosition"></param>
        /// <param name="blockType"></param>
        public void InstantiateBlockParticle(ParticleType particleType, Vector3 worldPosition, BlockType blockType = BlockType.AIR, bool roundToBlockPosition = false)
        {
            ParticleSystem particle = SpawnParticle(particleType, worldPosition, roundToBlockPosition);

            if (blockType != BlockType.AIR)
            {
                ParticleSystemRenderer particleSystemRenderer = particle.GetComponent<ParticleSystemRenderer>();
                particleSystemRenderer.mesh = CreateBlockParticleMeh(blockType);
            }

            particle.Play();
            StartCoroutine(DestroyBlockParticleEnumerator(particleType, particle, particle.main.duration));
        }

        /// <summary>
        /// Instantiate particle of type <see cref="ParticleType"/> at specified world position
        /// </summary>
        /// <param name="particleType"></param>
        /// <param name="worldPosition"></param>
        public void InstantiateParticle(ParticleType particleType, Vector3 worldPosition, bool roundToBlockPosition = false)
        {
            ParticleSystem particle = SpawnParticle(particleType, worldPosition, roundToBlockPosition);

            particle.Play();
            StartCoroutine(DestroyBlockParticleEnumerator(particleType, particle, particle.main.duration));
        }


        /// <summary>
        /// Instantiate particle GameObject
        /// </summary>
        private ParticleSystem SpawnParticle(ParticleType type, Vector3 blockPosition, bool centerBlockPosition = false)
        {
            ParticleSystem result = null;
            Vector3 position = centerBlockPosition ? new Vector3(blockPosition.x + 0.5f, blockPosition.y, blockPosition.z + 0.5f) : blockPosition;

            // if pool contains particle
            if (particlesPool[type].Count > 0)
            {
                result = particlesPool[type].Dequeue();
                result.transform.position = position;
                result.gameObject.SetActive(true);

                switch(type)
                {
                    case ParticleType.BULLET_HIT:
                        Vector3 direction = (Player.MouseLook.cameraTransform.position - position).normalized;
                        result.transform.rotation =  Quaternion.LookRotation(direction);
                        break;
                }

                return result;
            }

            switch (type)
            {
                case ParticleType.BLOCK_DESTROY:
                    result = Instantiate(onBlockDestroyParticle, position, Quaternion.identity).GetComponent<ParticleSystem>();
                    break;
                case ParticleType.BLOCK_PLACE:
                    result = Instantiate(onBlockPlaceParticle, position, Quaternion.identity).GetComponent<ParticleSystem>();
                    break;
                case ParticleType.BULLET_HIT:
                    result = Instantiate(onBulletHitParticle, position, Quaternion.identity).GetComponent<ParticleSystem>();
                    Vector3 direction = (Player.MouseLook.cameraTransform.position - position).normalized;
                    result.transform.rotation =  Quaternion.LookRotation(direction);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Get particle mesh by BlockType
        /// </summary>
        /// <param name="type">type of block</param>
        private static Mesh CreateBlockParticleMeh(BlockType type)
        {
            Mesh particleMesh;
            Block block = WorldData.GetBlockData(type);

            float offsetX = UnityEngine.Random.Range(0f, 1 - blockDestroyParticleUvSize) / Terrain.Blocks.BlockUVs.textureSize;
            float offsetY = UnityEngine.Random.Range(0f, blockDestroyParticleUvSize) / Terrain.Blocks.BlockUVs.textureSize;
            float size = blockDestroyParticleUvSize / Terrain.Blocks.BlockUVs.textureSize;
            
            if (particleMeshes.ContainsKey(type))
            {
                particleMesh = particleMeshes[type];

                particleMesh.uv = new Vector2[]
                {
                    block.sideUVs.uv0 + new float2(offsetX, offsetY),
                    block.sideUVs.uv0 + new float2(offsetX, offsetY + size),
                    block.sideUVs.uv0 + new float2(offsetX + size, offsetY + size),
                    block.sideUVs.uv0 + new float2(offsetX + size, offsetY)
                };

                particleMesh.RecalculateNormals();

                return particleMeshes[type];
            }

            particleMesh = new Mesh();

            particleMesh.vertices = new Vector3[]
            {
                new Vector3(0,0,0),
                new Vector3(0,1,0),
                new Vector3(1,1,0),
                new Vector3(1,0,0)
            };
            particleMesh.uv = new Vector2[]
            {
                block.sideUVs.uv0 + new float2(offsetX, offsetY),
                block.sideUVs.uv0 + new float2(offsetX, offsetY + size),
                block.sideUVs.uv0 + new float2(offsetX + size, offsetY + size),
                block.sideUVs.uv0 + new float2(offsetX + size, offsetY)
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

            particleMeshes.Add(type, particleMesh);
            return particleMesh;
        }

        #region IEnumerators

        /// <summary>
        /// Disable and add do pool particles after their lifetime
        /// </summary>
        private IEnumerator DestroyBlockParticleEnumerator(ParticleType type, ParticleSystem particle, float time)
        {
            yield return new WaitForSecondsRealtime(time);
            particlesPool[type].Enqueue(particle);
            particle.gameObject.SetActive(false);
        }

        /// <summary>
        /// Remove particles from pool if there are too many
        /// </summary>
        /// <param name="repeatTime">time between checks</param>
        private IEnumerator BlockParticlesPoolCleaner(float repeatTime)
        {
            var wait = new WaitForSecondsRealtime(repeatTime);
            while (true)
            {
                foreach (var queue in particlesPool.Values)
                {
                    if (queue.Count > targetPoolSize)
                        Destroy(queue.Dequeue());
                }

                yield return wait;
            }
        }

        #endregion
    }
}
