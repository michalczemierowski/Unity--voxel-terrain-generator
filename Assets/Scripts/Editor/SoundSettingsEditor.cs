#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using VoxelTG.Effects.SFX;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.CustomEditors
{
    [CustomEditor(typeof(WeaponEffectsController))]
    public class SoundSettingsEditor : Editor
    {
        WeaponEffectsController weaponFX;

        void OnEnable()
        {
            weaponFX = (WeaponEffectsController)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("RESET SOUND SETTINGS"))
            {
                weaponFX.shootSoundSettings = SoundSettings.DEFAULT;
            }
        }
    }
}

#endif