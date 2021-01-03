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
        private bool isUIModeActive;
        /// <summary>
        /// True if player using UI
        /// </summary>
        public static bool IsUIModeActive => Instance.isUIModeActive;

        /// <summary>
        /// True if player is using UI element (e.g. input field)
        /// </summary>
        public static bool IsUsingUIInput { get; set; }

        private ToggleableUI activeUiObject;

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

        private void Start()
        {
            // initialize objects that are disabled by default
            inventoryUI.Init();
        }

        private void OnDestroy()
        {
            // to make sure timeScale is not 0 when going back to main menu when in UI mode
            if (Instance == this)
                Time.timeScale = 1;
        }

        /// <summary>
        /// Toggle UI mode, when UI mode is active player is unable to move or perform actions outside UI.
        /// timeScale is set to 0 when UI mode is active
        /// </summary>
        /// <param name="active"></param>
        public static void ToggleUIMode(bool active, ToggleableUI toggleableUI = null)
        {
            if (toggleableUI == Instance.activeUiObject)
                return;

            // disable last active object before replacing with new one
            if (Instance.activeUiObject != null && Instance.activeUiObject.IsUIAcive)
                Instance.activeUiObject.CloseUI();

            Instance.activeUiObject = toggleableUI;
            Instance.isUIModeActive = active;

            Time.timeScale = active ? 0 : 1;
            Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = active;
        }
    }
}
