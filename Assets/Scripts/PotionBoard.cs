using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    public int width = 6;
    public int height = 8;
    public float SpacingX { get; private set; }
    public float SpacingY { get; private set; }

    public GameObject[] potions;
    public Node[,] potionBoard;
    public ArrayLayout layout;

    public static PotionBoard Instance { get; private set; }

    void Awake() { Instance = this; }

    void Start() { InitializeBoard(); }

    void InitializeBoard()
    {
        potionBoard = new Node[width, height];
        SpacingX = (float)(width - 1) / 2;
        SpacingY = ((float)(height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new(x - SpacingX, y - SpacingY);
                if (layout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node(false, null);
                }
                else
                {
                    int randomIndex = Random.Range(0, potions.Length);
                    GameObject newPotion = Instantiate(potions[randomIndex], position, Quaternion.identity);
                    newPotion.transform.parent = transform;
                    Potion potion = newPotion.GetComponent<Potion>();
                    potion.SetIndex(x, y);
                    potionBoard[x, y] = new Node(true, potion);
                }
            }

        if (CheckBoard())
        {
            // Uninstantiate game objects
            foreach (Node node in potionBoard)
            {
                if (node != null && node.potion != null)
                    Destroy(node.potion.gameObject);
            }
            InitializeBoard();
        }
    }

    public bool CheckBoard()
    {
        bool hasMatched = false;
        List<Potion> potionsToRemove = new();

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Node node = potionBoard[x, y];
                if (node.isUsable)
                {
                    if (!node.potion.isMatched)
                    {
                        MatchResult matchPotions = IsConnected(node.potion);
                        if (matchPotions.connectedPotions.Count >= 3)
                        {
                            // TODO complex matching
                            hasMatched = true;
                            potionsToRemove.AddRange(matchPotions.connectedPotions);
                            foreach (Potion p in matchPotions.connectedPotions)
                                p.isMatched = true;
                        }
                    }
                }
            }

        return hasMatched;
    }

    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new() { potion };

        // Check horizontal
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);
        if (connectedPotions.Count == 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };
        }
        else if (connectedPotions.Count > 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };
        }

        // Reset
        connectedPotions.Clear();
        connectedPotions.Add(potion);

        // Check vertical
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);
        if (connectedPotions.Count == 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedPotions.Count > 3)
        {
            return new MatchResult
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };
        }

        return new MatchResult
        {
            connectedPotions = connectedPotions,
            direction = MatchDirection.None
        };
    }

    void CheckDirection(Potion potion, Vector2Int direction, List<Potion> connectedPotions)
    {
        int x = potion.xIndex + direction.x;
        int y = potion.yIndex + direction.y;

        while (x >= 0 && x < width && y >= 0 && y < height)
        {
            Node node = potionBoard[x, y];
            if (!node.isUsable) break;
            if (!node.potion.isMatched && node.potion.potionType == potion.potionType)
            {
                connectedPotions.Add(node.potion);
                x += direction.x;
                y += direction.y;
            }
            else break;
        }
    }
}

public class MatchResult
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None,
}
