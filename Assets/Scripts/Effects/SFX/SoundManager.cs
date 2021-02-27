using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VoxelTG.Terrain;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Effects.SFX
{
    public class SoundManager : MonoBehaviour
    {
        private const string PATH_TO_ENVIROMENT_SFX = "sfx/enviroment/";
        private const string PATH_TO_ITEM_SFX = "sfx/item/";

        [SerializeField] private GameObject audioSourcePrefab;
        [SerializeField] private string audioClipsExtension = ".mp3";

        /// <summary>
        /// Audio clip cache (sound after first load will be stored here)
        /// </summary>
        private Dictionary<SoundType, AudioClip> audioCache = new Dictionary<SoundType, AudioClip>();

        /// <summary>
        /// Get adressables path to specified sound resource (used when loading resources)
        /// </summary>
        private string GetPathToAudioClip(SoundType soundType)
        {
            string result;
            switch ((int)soundType)
            {
                case int n when n < 1000:
                    result = PATH_TO_ENVIROMENT_SFX;
                    break;
                case int n when n >= 1000 && n < 2000:
                    result = PATH_TO_ITEM_SFX;
                    break;
                default:
                    result = string.Empty;
                    break;
            }

            return result + soundType + audioClipsExtension;
        }

        private void OnAudioClipLoadingComplete(SoundType soundType, AsyncOperationHandle<AudioClip> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var clip = handle.Result;
                audioCache[soundType] = clip;
            }
            else
                Debug.LogError(handle.OperationException.Message, this);
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
                // try to load audio clip an call method again when loading is complete
                System.Action onComplete = () => PlaySound(soundType, worldPosition, soundSettings);
                CacheSound(soundType, onComplete);

                return;
            }
            else
                clip = audioCache[soundType];

            if (clip == null)
                return;

            AudioSource audioSource = Instantiate(audioSourcePrefab, worldPosition, Quaternion.identity, transform).GetComponent<AudioSource>();

            // set audio source parent to player transform
            if (soundSettings.followPlayer)
                audioSource.transform.parent = Player.PlayerController.Instance.transform;

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

        /// <summary>
        /// Load AudioClip and save it in cache
        /// </summary>
        public void CacheSound(SoundType soundType, System.Action OnComplete = null)
        {
            if (!audioCache.ContainsKey(soundType))
            {
                // int value to not load it many times
                audioCache[soundType] = null;

                string path = GetPathToAudioClip(soundType);
                // call 'OnAudioClipLoadingComplete' on complete
                Addressables.LoadAssetAsync<AudioClip>(path).Completed += (handle) =>
                {
                    OnAudioClipLoadingComplete(soundType, handle);
                    OnComplete?.Invoke();
                };
            }
        }
    }
}
