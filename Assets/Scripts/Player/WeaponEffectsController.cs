using System.Collections;
using UnityEngine;
using VoxelTG.Effects.SFX;
using VoxelTG.Effects.VFX;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
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
    }

    public void OnShoot()
    {
        // particle
        if(shootParticle != null)
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
        //SoundManager.Instance.PlaySound(SoundType.DESTROY_WOOD, hitInfo.point, SoundSettings.DEFAULT);
        ParticleManager.InstantiateBlockParticle(ParticleType.BULLET_HIT, hitInfo.point, blockType);
    }

    private IEnumerator LightEnableDisableCoroutine()
    {
        shootLight.enabled = true;
        yield return new WaitForSeconds(shootLightTime);
        shootLight.enabled = false;
    }
}
