using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelTG.DebugUtils;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Player.Interactions
{
    public class FlashlightController : MonoBehaviour
    {
        [SerializeField] private Light[] flashLightModes;
        private int currentMode;

        public void NextFlashLightMode()
        {
            int nextMode = currentMode + 1 > flashLightModes.Length ? 0 : currentMode + 1;

            if(currentMode != 0)
                flashLightModes[currentMode-1].enabled = false;
            if(nextMode != 0)
            {
                flashLightModes[nextMode-1].enabled = true;
            }

            DebugConsole.AddDebugMessageStatic("FLASHLIGHT MODE SET TO: " + nextMode);
            currentMode = nextMode;
        }
    }
}
