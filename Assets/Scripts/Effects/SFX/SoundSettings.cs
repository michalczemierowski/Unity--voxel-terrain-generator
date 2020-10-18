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
        public int priority;
        public float volume;
        public float pitch;
        public float stereoPan;

        public bool loop;
        public float minDistance;
        public float maxDistance;

        public static readonly SoundSettings DEFAULT = new SoundSettings()
        {
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
