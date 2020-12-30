using UnityEngine;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Inventory
{
    /// <summary>
    /// Struct used to store item's special effects
    /// </summary>
    [System.Serializable]
    public struct ClothingBuff
    {
        [SerializeField] private ClothingBuffType buffType;
        public ClothingBuffType BuffType => buffType;

        [SerializeField] private float buffStrenght;
        public float BuffStrenght => buffStrenght;

        public ClothingBuff(ClothingBuffType buffType, float buffStrenght)
        {
            this.buffType = buffType;
            this.buffStrenght = buffStrenght;
        }
    }
}