using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    [SerializeField] private GameObject[] potions;
    [SerializeField] private GameObject cobwebs;
    [SerializeField] private GameObject selectedBorder;
    [SerializeField] private float refillDelay = 0f;
    [SerializeField] private float refillSpeed = 5.0f;
    [SerializeField] private float cascadeSpeed = 3.0f;
    [SerializeField] private float swapDuration = 0.1f;

    public Node[,] Nodes { get; private set; }
    public static PotionBoard Instance { get; private set; }
    private Potion selectedPotion;
    private Level level;
    public Potion SelectedPotion
    {
        get { return selectedPotion; }
        set
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.selectSFX);
            if (value)
            {
                selectedBorder.SetActive(true);
                selectedBorder.transform.position = value.transform.position;
            }
            else
                selectedBorder.SetActive(false);

            selectedPotion = value;
        }
    }
    public bool IsProcessing => isMatching || isCascading || IsMoving || IsSpawning;

    private bool isMatching, isCascading;
    private float spacingX, spacingY;
    bool IsMoving
    {
        get
        {
            foreach (Node node in Nodes)
                if (node.potion != null && node.potion.IsMoving)
                    return true;
            return false;
        }
    }
    bool IsSpawning
    {
        get
        {
            foreach (Node node in Nodes)
                if (node.potion != null && !node.potion.HasSpawned)
                    return true;
            return false;
        }
    }


    void Awake()
    {
        Instance = this;
        selectedBorder.SetActive(false);
    }

    void Start()
    {
        level = GameManager.Instance.CurrentLevel;
        InitializeBoard();
    }

    void InitializeBoard()
    {
        Nodes = new Node[level.width, level.height];
        spacingX = (float)(level.width - 1) / 2;
        spacingY = (float)(level.height - 1) / 2;

        for (int y = 0; y < level.height; y++)
            for (int x = 0; x < level.width; x++)
            {
                Vector2 position = new(x - spacingX, y - spacingY);

                if (level.Layout[x, level.height - y - 1])
                {
                    Nodes[x, y] = new Node();
                    GameObject newCobwebs = Instantiate(cobwebs, transform);
                    newCobwebs.transform.localPosition = position;
                }
                else
                {
                    int randomIndex = Random.Range(0, potions.Length);
                    GameObject newPotionObject = Instantiate(potions[randomIndex], transform);
                    newPotionObject.transform.localPosition = position;

                    Potion newPotion = newPotionObject.GetComponent<Potion>();
                    newPotion.SetIndex(x, y);
                    newPotion.HasSpawned = true;
                    Nodes[x, y] = new Node { isUsable = true, potion = newPotion };
                }
            }

        if (CheckBoard())
        {
            // Uninstantiate game Objects
            foreach (Node node in Nodes)
                if (node.potion != null)
                    Destroy(node.potion.gameObject);

            InitializeBoard();
        }
    }

    #region Board Logic
    public List<Potion> GetMatchedPotions()
    {
        foreach (Node node in Nodes)
            if (node.potion != null)
                node.potion.IsMatched = false;

        List<Potion> allMatchedPotions = new();

        for (int y = 0; y < level.height; y++)
            for (int x = 0; x < level.width; x++)
            {
                Node node = Nodes[x, y];
                if (node.isUsable && node.potion != null)
                    if (!node.potion.IsMatched)
                    {
                        MatchResult matchedPotions = IsConnected(node.potion);
                        if (matchedPotions.connectedPotions.Count >= 3)
                        {
                            matchedPotions = CheckSuperMatch(matchedPotions);

                            allMatchedPotions.AddRange(matchedPotions.connectedPotions);
                            foreach (Potion p in matchedPotions.connectedPotions)
                                p.IsMatched = true;
                        }
                    }
            }

        return allMatchedPotions;
    }

    public bool CheckBoard()
    {
        return GetMatchedPotions().Count > 0;
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

        while (x >= 0 && x < level.width && y >= 0 && y < level.height)
        {
            Node node = Nodes[x, y];
            if (!node.isUsable || node.potion == null) break;
            if (!node.potion.IsMatched && node.potion.potionType == potion.potionType)
            {
                connectedPotions.Add(node.potion);
                x += direction.x;
                y += direction.y;
            }
            else break;
        }
    }

    void OrderPotionsInHierarchy()
    {
        foreach (Node node in Nodes)
            if (node.potion != null)
                node.potion.transform.SetAsLastSibling();
    }
    #endregion

    #region Swap Logic
    public void SelectPotion(Potion potion)
    {
        if (IsProcessing || GameManager.Instance.GameOver) return;

        if (SelectedPotion == null) SelectedPotion = potion;
        else if (SelectedPotion == potion) SelectedPotion = null;
        else if (!IsAdjacentWithSelected(potion)) SelectedPotion = potion;
        else
        {
            SwapPotions(SelectedPotion, potion);
            StartCoroutine(CheckMatches(
                () => SwapPotions(SelectedPotion, potion)
            ));
        }
    }

    void SwapPotions(Potion potion1, Potion potion2)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.swapSFX);

        Node selectedNode = Nodes[potion1.xIndex, potion1.yIndex];
        Node targetNode = Nodes[potion2.xIndex, potion2.yIndex];

        // Swap nodes
        Nodes[potion1.xIndex, potion1.yIndex] = targetNode;
        Nodes[potion2.xIndex, potion2.yIndex] = selectedNode;

        // Swap indices
        int tempX = potion1.xIndex;
        int tempY = potion1.yIndex;
        potion1.SetIndex(potion2.xIndex, potion2.yIndex);
        potion2.SetIndex(tempX, tempY);

        StartCoroutine(SwapPotionsPhysically(potion1, potion2));
    }

    IEnumerator SwapPotionsPhysically(Potion potion1, Potion potion2)
    {
        Vector2 position1 = potion1.transform.localPosition;
        Vector2 position2 = potion2.transform.localPosition;
        StartCoroutine(potion1.MoveWithDuration(position2, swapDuration));
        StartCoroutine(potion2.MoveWithDuration(position1, swapDuration));

        while (potion1.IsMoving || potion2.IsMoving)
            yield return null;
    }

    IEnumerator CheckMatches(System.Action revertSwap)
    {
        GameManager.Instance.UseMove();
        while (IsMoving) yield return null;

        isMatching = true;
        if (CheckBoard())
        {
            SelectedPotion = null;
            isMatching = false;
            yield return RemoveAndRefill();
        }
        else
        {
            revertSwap();
            SelectedPotion = null;
            isMatching = false;
            yield break;
        }

        OrderPotionsInHierarchy();
    }

    bool IsAdjacentWithSelected(Potion potion)
    {
        if (SelectedPotion == null) return false;
        return Mathf.Abs(SelectedPotion.xIndex - potion.xIndex) + Mathf.Abs(SelectedPotion.yIndex - potion.yIndex) == 1;
    }
    #endregion

    #region Cascading Logic
    IEnumerator RemoveAndRefill()
    {
        while (CheckBoard())
        {
            while (isCascading || IsSpawning) yield return null;
            yield return RemoveMatchedPotions();
        }
    }

    IEnumerator RemoveMatchedPotions()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.matchSFX);
        isCascading = true;
        foreach (Potion potion in GetMatchedPotions())
        {
            Nodes[potion.xIndex, potion.yIndex].potion = null;
            Destroy(potion.gameObject);
            GameManager.Instance.AddPoints(1);
        }

        yield return new WaitForSeconds(refillDelay);


        for (int x = 0; x < level.width; x++)
            for (int y = 0; y < level.height; y++)
            {
                Node node = Nodes[x, y];
                if (node.potion == null && node.isUsable)
                    RefillPotion(x, y);
            }

        while (IsMoving) yield return null;
        isCascading = false;
    }

    void RefillPotion(int x, int y)
    {
        int yOffSet = 1;
        while (y + yOffSet < level.height && Nodes[x, y + yOffSet].potion == null && Nodes[x, y + yOffSet].isUsable)
            yOffSet++;

        if (y + yOffSet == level.height || !Nodes[x, y + yOffSet].isUsable)
            SpawnPotionAtTop(x);
        else
        {
            Potion potionAbove = Nodes[x, y + yOffSet].potion;

            // Move in memory
            potionAbove.SetIndex(x, y);
            Nodes[x, y].potion = potionAbove;
            Nodes[x, y + yOffSet].potion = null;

            // Move in scene
            Vector2 targetPosition = new(x - spacingX, y - spacingY);
            StartCoroutine(potionAbove.MoveWithSpeed(targetPosition, cascadeSpeed));
        }
    }

    void SpawnPotionAtTop(int x)
    {
        int targetRow = FindIndexOfLowestNull(x);

        // Create new potion
        int randomIndex = Random.Range(0, potions.Length);
        GameObject newPotionObject = Instantiate(potions[randomIndex], transform);
        newPotionObject.transform.localPosition = new Vector2(x - spacingX, level.height - 1 + targetRow - spacingY);

        // Set index and add to board
        Potion newPotion = newPotionObject.GetComponent<Potion>();
        newPotion.HasSpawned = false;
        newPotion.SetIndex(x, targetRow);
        Nodes[x, targetRow] = new Node { isUsable = true, potion = newPotion };

        // Move potion to target position
        Vector2 targetPosition = new(x - spacingX, targetRow - spacingY);
        StartCoroutine(MoveSpawnedPotion(newPotion, targetPosition));
    }

    IEnumerator MoveSpawnedPotion(Potion potion, Vector2 targetPosition)
    {
        while (isCascading) yield return null;
        yield return potion.MoveWithSpeed(targetPosition, refillSpeed);
        potion.HasSpawned = true;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.dropSFX);
    }

    int FindIndexOfLowestNull(int x)
    {
        for (int y = 0; y < level.height; y++)
            if (Nodes[x, y].potion == null && Nodes[x, y].isUsable)
                return y;

        Debug.LogError("No null found in column " + x);
        return level.height;
    }
    #endregion
}
