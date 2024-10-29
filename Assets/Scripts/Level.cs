using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject
{
    public int width;
    public int height;
    private bool[,] layout;

    public bool[,] Layout
    {
        get
        {
            layout ??= new bool[width, height];
            return layout;
        }

        set => layout = value;
    }
}
