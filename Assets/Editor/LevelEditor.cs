using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Level level = (Level)target;

        level.moves = EditorGUILayout.IntField("Moves", level.moves);
        level.goal = EditorGUILayout.IntField("Goal", level.goal);

        EditorGUILayout.Space();

        level.width = EditorGUILayout.IntField("Width", level.width);
        level.height = EditorGUILayout.IntField("Height", level.height);

        level.showLayout = EditorGUILayout.Foldout(level.showLayout, "Show Layout");
        if (level.showLayout)
        {
            for (int y = 0; y < level.height; y++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < level.width; x++)
                    level.Layout[x, y] = GUILayout.Toggle(level.Layout[x, y], GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }
        }

        if (GUI.changed)
            EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssetIfDirty(level);
    }
}