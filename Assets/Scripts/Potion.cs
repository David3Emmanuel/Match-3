using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public PotionType potionType;
    public int xIndex, yIndex;

    public bool IsMoving { get; private set; }
    public bool IsMatched { get; set; }
    public bool HasSpawned { get; set; }

    public void SetIndex(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    public IEnumerator MoveWithDuration(Vector2 targetPosition, float duration)
    {
        IsMoving = true;
        Vector2 startPosition = transform.position;
        float time = 0f;

        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        IsMoving = false;
    }

    public IEnumerator MoveWithSpeed(Vector2 targetPosition, float speed)
    {
        if (this == null) yield break;

        IsMoving = true;
        Vector2 startPosition = transform.position;
        float time = 0f;

        while (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            time += Time.deltaTime;
            yield return null;
            if (this == null) break;
        }

        if (this != null) transform.position = targetPosition;
        IsMoving = false;
    }

    void OnMouseDown()
    {
        PotionBoard.Instance.SelectPotion(this);
        Debug.DrawLine(Vector3.zero, transform.position, Color.red, 1f);
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
