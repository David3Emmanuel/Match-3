using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject, ISerializationCallbackReceiver
{
    public int width = 6;
    public int height = 8;
    public int moves = 10;
    public int goal = 30;
    private bool[,] layout;
    public SerializableLayout serializedLayout;
    public bool showLayout;

    public bool[,] Layout
    {
        get
        {
            if (layout == null || layout.GetLength(0) != width || layout.GetLength(1) != height)
                layout = new bool[width, height];
            return layout;
        }

        set => layout = value;
    }

    public void OnBeforeSerialize()
    {
        serializedLayout = new()
        {
            rows = new SerializableLayoutRow[height]
        };

        for (int y = 0; y < height; y++)
        {
            serializedLayout.rows[y].row = new bool[width];
            for (int x = 0; x < width; x++)
                serializedLayout.rows[y].row[x] = Layout[x, y];
        }
    }

    public void OnAfterDeserialize()
    {
        if (serializedLayout.rows == null)
            return;

        height = serializedLayout.rows.Length;
        width = serializedLayout.rows[0].row.Length;
        Layout = new bool[width, height];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                Layout[x, y] = serializedLayout.rows[y].row[x];
    }
}

[Serializable]
public struct SerializableLayoutRow
{
    public bool[] row;
}

[Serializable]
public struct SerializableLayout
{
    public SerializableLayoutRow[] rows;
}
