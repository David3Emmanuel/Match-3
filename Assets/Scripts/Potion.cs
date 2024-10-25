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

    public Potion(int xIndex, int yIndex)
    {
        this.xIndex = xIndex;
        this.yIndex = yIndex;
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
