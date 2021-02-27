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
    public enum SoundType : short
    {
        // enviroment
        NONE,
        PLACE_STONE,
        DESTROY_STONE,
        DESTROY_WOOD,

        // item
        RIFLE_AK74_SHOT = 1000,
        PISTOL_M1911_SHOT
    }
}
