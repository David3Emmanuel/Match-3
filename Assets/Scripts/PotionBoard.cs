using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    // Editor fields
    public int width = 6;
    public int height = 8;
    public GameObject[] potions;
    public ArrayLayout layout;

    // Public properties
    public Node[,] Nodes { get; private set; }
    public static PotionBoard Instance { get; private set; }
    public Potion SelectedPotion { get; set; }

    private bool isSwapping, isMatching;
    private bool IsProcessing => isSwapping || isMatching;

    void Awake() { Instance = this; }

    void Start() { InitializeBoard(); }

    void InitializeBoard()
    {
        Nodes = new Node[width, height];
        float spacingX = (float)(width - 1) / 2;
        float spacingY = ((float)(height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new(x - spacingX, y - spacingY);
                if (layout.rows[y].row[x])
                {
                    Nodes[x, y] = new Node(false, null);
                }
                else
                {
                    int randomIndex = Random.Range(0, potions.Length);
                    GameObject newPotion = Instantiate(potions[randomIndex], position, Quaternion.identity);
                    newPotion.transform.parent = transform;
                    Potion potion = newPotion.GetComponent<Potion>();
                    potion.SetIndex(x, y);
                    Nodes[x, y] = new Node(true, potion);
                }
            }

        if (CheckBoard())
        {
            // Uninstantiate game Pbjects
            foreach (Node node in Nodes)
            {
                if (node != null && node.potion != null)
                    Destroy(node.potion.gameObject);
            }
            InitializeBoard();
        }
    }

    #region Board Logic
    public bool CheckBoard()
    {
        bool hasMatched = false;
        List<Potion> potionsToRemove = new();

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Node node = Nodes[x, y];
                if (node.isUsable)
                {
                    if (!node.potion.IsMatched)
                    {
                        MatchResult matchedPotions = IsConnected(node.potion);
                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            matchedPotions = CheckSuperMatch(matchedPotions);

                            hasMatched = true;
                            potionsToRemove.AddRange(matchedPotions.connectedPotions);
                            foreach (Potion p in matchedPotions.connectedPotions)
                                p.IsMatched = true;
                        }
                    }
                }
            }

        return hasMatched;
    }

    MatchResult CheckSuperMatch(MatchResult matchedPotions)
    {
        Vector2Int direction1, direction2;
        if (
            matchedPotions.direction == MatchDirection.Horizontal ||
            matchedPotions.direction == MatchDirection.LongHorizontal
        )
        {
            direction1 = new Vector2Int(0, 1);
            direction2 = new Vector2Int(0, -1);
        }
        else if (
            matchedPotions.direction == MatchDirection.Vertical ||
            matchedPotions.direction == MatchDirection.LongVertical
        )
        {
            direction1 = new Vector2Int(1, 0);
            direction2 = new Vector2Int(-1, 0);
        }
        else return matchedPotions;

        foreach (Potion potion in matchedPotions.connectedPotions)
        {
            List<Potion> extraConnections = new();
            CheckDirection(potion, direction1, extraConnections);
            CheckDirection(potion, direction2, extraConnections);

            if (extraConnections.Count >= 2)
            {
                matchedPotions.connectedPotions.AddRange(extraConnections);
                matchedPotions.direction = MatchDirection.Super;
                return matchedPotions;
            }
        }

        return matchedPotions;
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
            Node node = Nodes[x, y];
            if (!node.isUsable) break;
            if (!node.potion.IsMatched && node.potion.potionType == potion.potionType)
            {
                connectedPotions.Add(node.potion);
                x += direction.x;
                y += direction.y;
            }
            else break;
        }
    }
    #endregion

    #region Swap Logic
    public void SelectPotion(Potion potion)
    {
        if (IsProcessing) return;

        if (SelectedPotion == null) SelectedPotion = potion;
        else if (SelectedPotion == potion) SelectedPotion = null;
        else if (!IsAdjacentWithSelected(potion)) SelectedPotion = potion;
        else
        {
            isSwapping = true;
            Node selectedNode = Nodes[SelectedPotion.xIndex, SelectedPotion.yIndex];
            Node targetNode = Nodes[potion.xIndex, potion.yIndex];

            // Swap nodes
            Nodes[SelectedPotion.xIndex, SelectedPotion.yIndex] = targetNode;
            Nodes[potion.xIndex, potion.yIndex] = selectedNode;

            // Swap indices
            int tempX = SelectedPotion.xIndex;
            int tempY = SelectedPotion.yIndex;
            SelectedPotion.SetIndex(potion.xIndex, potion.yIndex);
            potion.SetIndex(tempX, tempY);

            StartCoroutine(SwapPotions(SelectedPotion, potion));
            StartCoroutine(CheckMatches(
                () => StartCoroutine(SwapPotions(SelectedPotion, potion))
            ));
        }
    }

    IEnumerator SwapPotions(Potion potion1, Potion potion2)
    {
        Vector2 position1 = potion1.transform.position;
        Vector2 position2 = potion2.transform.position;
        StartCoroutine(potion1.Move(position2, 0.1f));
        StartCoroutine(potion2.Move(position1, 0.1f));

        while (potion1.IsMoving || potion2.IsMoving)
            yield return null;

        SelectedPotion = null;
        isSwapping = false;
    }

    IEnumerator CheckMatches(System.Action revertSwap)
    {
        while (isSwapping) yield return null;

        isMatching = true;
        if (CheckBoard())
        { }
        else revertSwap();

        isMatching = false;
    }

    bool IsAdjacentWithSelected(Potion potion)
    {
        if (SelectedPotion == null) return false;
        return Mathf.Abs(SelectedPotion.xIndex - potion.xIndex) + Mathf.Abs(SelectedPotion.yIndex - potion.yIndex) == 1;
    }
    #endregion
}

public struct MatchResult
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
