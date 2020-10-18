using System.Collections.Generic;
using UnityEngine;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Effects.SFX
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        [SerializeField] private GameObject audioSourcePrefab;

        [Header("Path settings")]
        [SerializeField] private string pathToSFX = "SFX";
        [SerializeField] private string enviromentDirectoryName = "enviroment";
        [SerializeField] private string itemsDirectoryName = "items";

        private Dictionary<SoundType, AudioClip> audioCache = new Dictionary<SoundType, AudioClip>();

        private void Awake()
        {
            if (Instance)
                Destroy(this);
            else
                Instance = this;
        }

        private string GetPathToResource(SoundType soundType)
        {
            string result = pathToSFX + "/";
            int soundTypeInt = (int)soundType;

            if (soundTypeInt < 100)
                result += enviromentDirectoryName;
            else if (soundTypeInt > 99 && soundTypeInt < 200)
                result += itemsDirectoryName;

            result += "/" + soundType;
            return result;
        }

        /// <summary>
        /// Instantiate audio source and play sound
        /// </summary>
        public void PlaySound(BlockType blockType, Vector3 worldPosition, SoundSettings soundSettings)
        {
            if (blockType == BlockType.AIR)
                return;

            PlaySound(WorldData.GetBlockData(blockType).soundType, worldPosition, soundSettings);
        }

        /// <summary>
        /// Instantiate audio source and play sound
        /// </summary>
        public void PlaySound(SoundType soundType, Vector3 worldPosition, SoundSettings soundSettings)
        {
            AudioClip clip;
            if (!audioCache.ContainsKey(soundType))
            {
                clip = Resources.Load<AudioClip>(GetPathToResource(soundType));
                Debug.Log(clip == null);
                if (clip != null)
                {
                    audioCache.Add(soundType, clip);
                }
                else
                    return;
            }
            else
                clip = audioCache[soundType];

            AudioSource audioSource = Instantiate(audioSourcePrefab, worldPosition, Quaternion.identity, transform).GetComponent<AudioSource>();

            // apply settings
            audioSource.priority = soundSettings.priority;
            audioSource.volume = soundSettings.volume;
            audioSource.pitch = soundSettings.pitch;
            audioSource.panStereo = soundSettings.stereoPan;
            audioSource.loop = soundSettings.loop;
            audioSource.minDistance = soundSettings.minDistance;
            audioSource.maxDistance = soundSettings.maxDistance;

            audioSource.clip = clip;
            audioSource.Play();
            Destroy(audioSource.gameObject, clip.length);
        }
    }
}
