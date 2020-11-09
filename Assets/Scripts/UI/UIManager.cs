using UnityEngine;
using UnityEngine.UI;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [SerializeField] private GameObject inWaterOverlay;
        public static GameObject InWaterOverlay => Instance.inWaterOverlay;

        [SerializeField] private Image miningProgressImage;
        public static Image MiningProgressImage => Instance.miningProgressImage;

        [SerializeField] private InventoryUI inventoryUI;
        public static InventoryUI InventoryUI => Instance.inventoryUI;

        private void Awake()
        {
            if (Instance)
                Destroy(this);
            else
                Instance = this;
        }
    }
}
