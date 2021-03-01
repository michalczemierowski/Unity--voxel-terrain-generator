/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
using System.Collections;

namespace VoxelTG.Terrain
{
    public struct BlockSettings
    {
        private const int arraySize = 1;
        private BitArray settingsArray;

        public void Init()
        {
            settingsArray = new BitArray(arraySize);
        }

        public bool CanPlaceGrass
        {
            get => settingsArray.Get(0);
            set
            {
                settingsArray.Set(0, value);
            }
        }
    }
}
