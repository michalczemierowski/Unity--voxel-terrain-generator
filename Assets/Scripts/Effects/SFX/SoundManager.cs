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
        [SerializeField] private GameObject audioSourcePrefab;

        [Header("Path settings")]
        [Tooltip("Path to resource folder containing SFX assets (parent folder for all directories below)")]
        [SerializeField] private string pathToSFX = "SFX";
        [Tooltip("Path to resource folder containing enviroment sounds (releative to 'PathToSFX' path)")]
        [SerializeField] private string enviromentDirectoryName = "enviroment";
        [Tooltip("Path to resource folder containing item sounds (releative to 'PathToSFX' path)")]
        [SerializeField] private string itemsDirectoryName = "items";

        /// <summary>
        /// Audio clip cache (sound after first load will be stored here)
        /// </summary>
        private Dictionary<SoundType, AudioClip> audioCache = new Dictionary<SoundType, AudioClip>();

        /// <summary>
        /// Get path to specified sound resource (used when loading resources)
        /// </summary>
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
