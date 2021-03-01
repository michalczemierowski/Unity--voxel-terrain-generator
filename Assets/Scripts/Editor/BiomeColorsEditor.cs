using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;
using VoxelTG.Config;
using VoxelTG.Terrain;

/*
 * Micha³ Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.CustomEditors
{
    [CustomEditor(typeof(BiomeColorsObject))]
    public class BiomeColorsEditor : Editor
    {
        private AnimBool[] foldoutBools;
        private SerializedProperty biomeTypes;
        private SerializedProperty biomeColors;

        private void OnEnable()
        {
            biomeTypes = serializedObject.FindProperty("biomeTypes");
            biomeColors = serializedObject.FindProperty("biomeColors");

            BiomeType[] allBiomes = (BiomeType[])System.Enum.GetValues(typeof(BiomeType));
            if(biomeTypes.arraySize < allBiomes.Length)
            {
                for (int i = 0; i < allBiomes.Length; i++)
                {
                    biomeTypes.InsertArrayElementAtIndex(i);
                    biomeTypes.GetArrayElementAtIndex(i).enumValueIndex = i;

                    if(i >= biomeColors.arraySize)
                    {
                        biomeColors.InsertArrayElementAtIndex(i);
                        biomeColors.GetArrayElementAtIndex(i).colorValue = Color.green;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            foldoutBools = new AnimBool[allBiomes.Length];
            for (int i = 0; i < foldoutBools.Length; i++)
            {
                foldoutBools[i] = new AnimBool(false);
                foldoutBools[i].valueChanged.AddListener(new UnityAction(base.Repaint));
            }
        }

        public override void OnInspectorGUI()
        {
            for (int i = 0; i < biomeTypes.arraySize; i++)
            {
                var type = biomeTypes.GetArrayElementAtIndex(i);
                foldoutBools[i].target = EditorGUILayout.Foldout(foldoutBools[i].target, $"Biome: {(BiomeType)type.enumValueIndex}");
                using (var group = new EditorGUILayout.FadeGroupScope(foldoutBools[i].faded))
                {
                    if (group.visible)
                    {
                        EditorGUI.indentLevel = 1;
                        var color = biomeColors.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(type, new GUIContent("Biome: "));
                        EditorGUILayout.PropertyField(color, new GUIContent("Color: "));
                        EditorGUI.indentLevel = 0;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}