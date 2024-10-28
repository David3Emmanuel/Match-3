using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject
{
    public int width = 6;
    public int height = 8;
    public ArrayLayout layout;
}
