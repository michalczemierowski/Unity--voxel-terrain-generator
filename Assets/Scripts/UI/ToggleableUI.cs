using UnityEngine;

/*
* Michał Czemierowski
* https://github.com/michalczemierowski
*/
namespace VoxelTG.UI
{
    public abstract class ToggleableUI : MonoBehaviour
    {
        public abstract bool IsUIAcive { get; }

        public virtual void OpenUI()
        {
            if(UIManager.Instance)
                UIManager.ToggleUIMode(true, this);
            else
                Debug.LogError("Unable to use UI methods. UIManager is missing", this);
        }

        public virtual void CloseUI()
        {
            if(UIManager.Instance)
                UIManager.ToggleUIMode(false);
            else
                Debug.LogError("Unable to use UI methods. UIManager is missing", this);
        }

        /// <summary>
        /// Enable/disable UI
        /// </summary>
        public virtual void ToggleUI()
        {
            if(IsUIAcive)
                CloseUI();
            else 
                OpenUI();
        }
    }
}
