/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Effects.SFX
{
    /// <summary>
    /// enum containing all possible sound types
    /// <0, 99> - enviroment
    /// <100, 199> - items
    /// </summary>
    public enum SoundType : byte
    {
        NONE,
        PLACE_STONE,
        DESTROY_STONE,
        DESTROY_WOOD,

        RIFLE_AK74_SHOOT = 100,
    }
}
