/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using UnityEngine.UI;

namespace VoxelTG.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public Image miningProgressImage;
        public InventoryUI inventoryUI;

        private void Awake()
        {
            if (Instance)
                Destroy(this);
            else
                Instance = this;
        }
    }
}
