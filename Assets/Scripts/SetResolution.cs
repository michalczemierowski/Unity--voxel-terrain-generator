/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;

namespace ResolutionSettings
{
    public class SetResolution : MonoBehaviour
    {
        public static bool showDialog { get; private set; }

        public ResolutionPreset[] resolutionPresets = new ResolutionPreset[]
        {
            new ResolutionPreset()
            {
                name = "HD",
                width = 1280,
                height = 720
            },
            new ResolutionPreset()
            {
                name = "FullHD",
                width = 1920,
                height = 1080
            },
        };

        private Rect showDialogButtonRect;
        private Rect dialogPanelRect;
        private Rect setResolutionButtonRect;
        private Rect TAGlabelRect, dropdownSelectionRect, qualityPresetsRect;

        private int selectedQuality, selectedResolution;

        private string TAG = "[ResolutionDialog] ";

        private void Awake()
        {
            showDialogButtonRect = new Rect(0, 0, 16, 16);
            dialogPanelRect = new Rect(showDialogButtonRect.xMax, showDialogButtonRect.yMax, 240, 360);

            setResolutionButtonRect = new Rect(dialogPanelRect.xMin + 8, dialogPanelRect.yMax - 38, dialogPanelRect.width - 16, 30);
            qualityPresetsRect = new Rect(dialogPanelRect.xMin + 8, setResolutionButtonRect.yMax - 158, dialogPanelRect.width - 16, 120);

            TAGlabelRect = new Rect(dialogPanelRect.xMin + 8, dialogPanelRect.yMin + 8, dialogPanelRect.width - 16, 30);
            dropdownSelectionRect = new Rect(dialogPanelRect.xMin + 8, dialogPanelRect.yMin + 8, dialogPanelRect.width - 16, 120);

            selectedQuality = QualitySettings.GetQualityLevel();
            Debug.Log($"{selectedQuality}  {QualitySettings.GetQualityLevel()}");
        }

        private void OnGUI()
        {
            if (GUI.Button(showDialogButtonRect, "X"))
                showDialog = !showDialog;

            if (!showDialog) return;

            GUI.Box(dialogPanelRect, string.Empty);

            GUI.Label(TAGlabelRect, TAG);

            string[] names = QualitySettings.names;

            float space = 5f;
            float width = setResolutionButtonRect.width / 3 - space;
            int rows = Mathf.CeilToInt((float)names.Length / 3);

            int counter = 0;

            selectedQuality = GUI.SelectionGrid(qualityPresetsRect, selectedQuality, names, 3);

            //Rect qualitySettings = new Rect(setResolutionButtonRect.xMin + space/2, setResolutionButtonRect.yMin - (rows * setResolutionButtonRect.height) - 8 - (space * rows), width, setResolutionButtonRect.height);
            //for (int i = 0; i < names.Length; i++)
            //{
            //    if (GUI.Button(qualitySettings, names[i]))
            //    {
            //        Debug.Log(TAG + "Set quality level to " + names[i]);
            //        QualitySettings.SetQualityLevel(i, true);
            //    }

            //    qualitySettings.x += width + 5;
            //    counter++;
            //    if (counter % 3 == 0)
            //    {
            //        qualitySettings.y += setResolutionButtonRect.height + space;
            //        qualitySettings.x = setResolutionButtonRect.xMin + space/2;
            //    }
            //}

            if (GUI.Button(setResolutionButtonRect, "Apply settings"))
            {
                QualitySettings.SetQualityLevel(selectedQuality, true);
                Debug.Log($"{selectedQuality}  {QualitySettings.GetQualityLevel()}");
            }

            //GUI.Label(new Rect(10, 10, 100, 100), "test");

            //if (GUI.Button(setResolutionButtonRect, "TestButton"))
            //{
            //    if (!showDialog) ShowDialog();
            //    else DisableDialog();
            //}
        }


        #region public static methods

        public static void ShowDialog()
        {
            showDialog = true;
        }

        public static void DisableDialog()
        {
            showDialog = false;
        }

        #endregion
    }

    [System.Serializable]
    public struct ResolutionPreset
    {
        public string name;
        public int width, height;

        //public ResolutionPreset()
        //{
        //    this.name = "resolution_preset";
        //    this.width = 0;
        //    this.height = 0;
        //}

        //public ResolutionPreset(string name, int width, int height)
        //{
        //    this.name = name;
        //    this.width = width;
        //    this.height = height;
        //}
    }
}
