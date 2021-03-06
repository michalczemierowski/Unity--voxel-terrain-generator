﻿using System.Collections;
using UnityEngine;
using VoxelTG.Effects.SFX;
using VoxelTG.Effects.VFX;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Interactions
{
    /// <summary>
    /// Class used to handle animations, sound and particles when using weapons
    /// </summary>
    public class WeaponEffectsController : MonoBehaviour
    {
        private Animator animator;
        public Animator Animator
        {
            get
            {
                if (!animator)
                {
                    Debug.LogWarning("There is no animator on " + gameObject.name, this);
                }

                return animator;
            }
        }

        [SerializeField] private ParticleSystem shootParticle;
        [SerializeField] private Light shootLight;
        [SerializeField] private float shootLightTime = 0.05f;
        [SerializeField] private SoundType shootSoundType;

        public SoundSettings shootSoundSettings;

        private void Start()
        {
            animator = GetComponent<Animator>();
            // cache sound
            World.SoundManager.CacheSound(shootSoundType);
        }

        public void OnShoot()
        {
            // particle
            if (shootParticle != null)
                shootParticle.Play();

            // light
            if (shootLight != null)
                StartCoroutine(nameof(LightEnableDisableCoroutine));

            // sound
            if (shootSoundType != SoundType.NONE)
                World.SoundManager.PlaySound(shootSoundType, transform.position, shootSoundSettings);
        }

        /// <summary>
        /// Called when bullet hits ground
        /// </summary>
        public void OnBulletHitTerrain(RaycastHit hitInfo, Chunk chunk, BlockPosition blockPosition, BlockType blockType)
        {
            World.SoundManager.PlaySound(blockType, hitInfo.point, SoundSettings.DEFAULT);
            World.ParticleManager.InstantiateBlockParticle(ParticleType.BULLET_HIT, hitInfo.point, blockType);
        }

        private IEnumerator LightEnableDisableCoroutine()
        {
            shootLight.enabled = true;
            yield return new WaitForSeconds(shootLightTime);
            shootLight.enabled = false;
        }
    }
}