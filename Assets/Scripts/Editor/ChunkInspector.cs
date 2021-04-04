using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VoxelTG.Terrain;
using VoxelTG;

[CustomEditor(typeof(Chunk))]
public class ChunkInspector : Editor
{
    private Chunk target;

    private GUIStyle neighbourFound;
    private GUIStyle neighbourNotFound;

    private void OnEnable()
    {
        target = serializedObject.targetObject as Chunk;

        neighbourFound = new GUIStyle(EditorStyles.miniButton)
        {
            fontSize = 20,
            onHover = new GUIStyleState()
            {
                textColor = new Color(0, 0.5f, 0),
                background = Texture2D.linearGrayTexture
            }
        };
        neighbourFound.fixedWidth = 50;
        neighbourFound.fixedHeight = 50;
        neighbourFound.normal.textColor = new Color(0, 1, 0);

        neighbourNotFound = new GUIStyle(EditorStyles.miniButton)
        {
            fontSize = 20
        };
        neighbourNotFound.fixedWidth = 50;
        neighbourNotFound.fixedHeight = 50;
        neighbourNotFound.normal.textColor = Color.red;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("Neighbour chunks: ");
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();

        if (Exists(Direction.NW))
        {
            if(GUILayout.Button("NW", neighbourFound))
                Selection.activeGameObject = Get(Direction.NW);
        }
        else
            GUILayout.Button("NW", neighbourNotFound);

        if (Exists(Direction.N))
        {
            if(GUILayout.Button("N", neighbourFound))
                Selection.activeGameObject = Get(Direction.N);
        }
        else
            GUILayout.Button("N", neighbourNotFound);

        if (Exists(Direction.NE))
        {
            if(GUILayout.Button("NE", neighbourFound))
                Selection.activeGameObject = Get(Direction.NE);
        }
        else
            GUILayout.Button("NE", neighbourNotFound);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (Exists(Direction.W))
        {
            if(GUILayout.Button("W", neighbourFound))
                Selection.activeGameObject = Get(Direction.W);
        }
        else
            GUILayout.Button("W", neighbourNotFound);

        GUILayout.Button("", neighbourFound);

        if (Exists(Direction.E))
        {
            if (GUILayout.Button("E", neighbourFound))
                Selection.activeGameObject = Get(Direction.E);
        }
        else
            GUILayout.Button("E", neighbourNotFound);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (Exists(Direction.SW))
        {
            if(GUILayout.Button("SW", neighbourFound))
                Selection.activeGameObject = Get(Direction.SW);
        }
        else
            GUILayout.Button("SW", neighbourNotFound);

        if (Exists(Direction.S))
        {
            if(GUILayout.Button("S", neighbourFound))
                Selection.activeGameObject = Get(Direction.S);
        }
        else
            GUILayout.Button("S", neighbourNotFound);

        if (Exists(Direction.SE))
        {
            if(GUILayout.Button("SE", neighbourFound))
                Selection.activeGameObject = Get(Direction.S);
        }
        else
            GUILayout.Button("SE", neighbourNotFound);

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        bool Exists(Direction dir)
        {
            return target.NeighbourChunks != null && target.NeighbourChunks[dir] != null;
        }

        GameObject Get(Direction dir)
        {
            return target.NeighbourChunks[dir].gameObject;
        }
    }
}
