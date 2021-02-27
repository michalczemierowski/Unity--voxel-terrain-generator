/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    public enum ItemType
    {
        // ITEMS - <2; 999>
        NONE = 0,
        MATERIAL = 1,
        AMMO_PISTOL,
        AMMO_RIFLE,
        AMMO_SHOTGUN,
        AMMO_SNIPER,

        // TOOLS - <1000; 1999>
        AXE = 1000,

        // WEAPONS - <2000; 2999>
        RIFLE_AK74 = 2000,
        SHOTGUN_M4,
        PISTOL_M1911,

        // CLOTHES - <3000+>
        CLOTHING_TEST = 3000,
    }
}