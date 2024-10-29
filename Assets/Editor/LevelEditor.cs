using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Level level = (Level)target;

        level.width = EditorGUILayout.IntField("Width", level.width);
        level.height = EditorGUILayout.IntField("Height", level.height);

        bool[,] currentLayout = level.Layout;

        if (currentLayout.GetLength(0) != level.width || currentLayout.GetLength(1) != level.height)
            level.Layout = new bool[level.width, level.height];

        for (int y = 0; y < level.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < level.width; x++)
                level.Layout[x, y] = GUILayout.Toggle(level.Layout[x, y], GUIContent.none);
            EditorGUILayout.EndHorizontal();
        }

        if (GUI.changed)
            EditorUtility.SetDirty(level);
    }
}