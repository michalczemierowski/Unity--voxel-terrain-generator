using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Effects.SFX
{
    /// <summary>
    /// Settings for playing sound from SoundManager
    /// </summary>
    [System.Serializable]
    public class SoundSettings
    {
        [Tooltip("Should AudioSource move with player.")]
        public bool followPlayer;

        [UnityEngine.Space]
        
        [Tooltip("Determines the priority of this audio source among all the ones that coexist in the scene. (Priority: 0 = most important. 256 = least important. Default = 128.).")]
        public int priority;
        [Tooltip("How loud the sound is.")]
        public float volume;
        [Tooltip("Amount of change in pitch due to slowdown/speed up of the Audio Clip. Value 1 is normal playback speed.")]
        public float pitch;
        [Tooltip("Sets the position in the stereo field of 2D sounds.")]
        public float stereoPan;
        [Tooltip("Enable this to make the Audio Clip loop when it reaches the end.")]
        public bool loop;
        [Tooltip("Within the MinDistance, the sound will stay at loudest possible. Outside MinDistance it will begin to attenuate.")]
        public float minDistance;
        [Tooltip("The distance where the sound stops attenuating at. Beyond this point it will stay at the volume it would be at MaxDistance units from the listener and will not attenuate any more.")]
        public float maxDistance;

        public static readonly SoundSettings DEFAULT = new SoundSettings()
        {
            followPlayer = false,
            
            priority = 128,
            volume = 1,
            pitch = 1,
            stereoPan = 0,
            loop = false,
            minDistance = 8,
            maxDistance = 64,
        };
    }
}
