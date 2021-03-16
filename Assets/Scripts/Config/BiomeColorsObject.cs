using UnityEngine;
using VoxelTG.Terrain;

/*
 * Micha³ Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Config
{
    [CreateAssetMenu(fileName = "Biome Colors Confg", menuName = "Scriptable Objects/Config/Biome Colors")]
    public class BiomeColorsObject : ScriptableObject
    {
        [SerializeField] private BiomeType[] biomeTypes;
        [SerializeField] private Color[] biomeColors;

        public Color GetBiomeColor(BiomeType biomeType)
        {
            int index = (int)biomeType;
            if (biomeColors.Length > index)
                return biomeColors[index];

            return Color.magenta;
        }

        public Color[] GetBiomeColors()
        {
            return biomeColors;
        }
    }
}
