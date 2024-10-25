using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    public int width = 6;
    public int height = 8;
    public float spacingX { get; private set; }
    public float spacingY { get; private set; }

    public GameObject[] potions;
    public Node[,] potionBoard;
    public ArrayLayout layout;
    
    public static PotionBoard Instance { get; private set; }

    void Awake() { Instance = this; }

    void Start() { InitializeBoard(); }

    void InitializeBoard()
    {
        potionBoard = new Node[width, height];
        spacingX = (float)(width-1) / 2;
        spacingY = ((float) (height-1) / 2) + 1;

        for (int y = 0; y < height; y++) {
        for (int x = 0; x < width; x++) {
            Vector2 position = new Vector2(x - spacingX, y - spacingY);
            if (layout.rows[y].row[x]) {
                potionBoard[x, y] = new Node(false, null);
            }
            else {
            int randomIndex = Random.Range(0, potions.Length);
            GameObject newPotion = Instantiate(potions[randomIndex], position, Quaternion.identity);
            newPotion.transform.parent = transform;
            Potion potion = newPotion.GetComponent<Potion>();
            potion.SetIndex(x, y);
            potionBoard[x, y] = new Node(true, potion);
            }
        }}
    }
}
