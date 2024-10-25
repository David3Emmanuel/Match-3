using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public PotionType potionType;
    public int xIndex, yIndex;
    public bool isMatched;

    private Vector2 currentPosition, targetPosition;
    public bool isMoving;

    public void SetIndex(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
}

public enum PotionType
{
    Red,
    Blue,
    Purple,
    Green,
    White,
}
