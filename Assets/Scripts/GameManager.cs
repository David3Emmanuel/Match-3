using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject backgroundPanel, victoryPanel, losePanel;
    public TextMeshProUGUI pointsText, movesText, winSummaryText, loseSummaryText;
    public int moves, goal;

    public static GameManager Instance { get; private set; }
    private int points;
    public bool GameOver { get; private set; }

    void Awake() { Instance = this; }

    void Start() { Initialize(moves, goal); }

    public void Initialize(int moves, int goal)
    {
        this.moves = moves;
        this.goal = goal;
        movesText.text = "Moves: " + moves;
        pointsText.text = points + " / " + goal;
    }

    public void UseMove()
    {
        moves--;
        movesText.text = "Moves: " + moves;

        if (moves <= 0) StartCoroutine(LoseGame());
    }

    public void AddPoints(int pointsGained)
    {
        points += pointsGained;
        pointsText.text = points + " / " + goal;

        if (points >= goal) StartCoroutine(WinGame());
    }

    IEnumerator WinGame()
    {
        while (PotionBoard.Instance.IsProcessing)
            yield return null;

        GameOver = true;
        backgroundPanel.SetActive(true);
        victoryPanel.SetActive(true);
        winSummaryText.text = $"You won in {moves} moves";
    }

    IEnumerator LoseGame()
    {
        while (PotionBoard.Instance.IsProcessing)
            yield return null;

        GameOver = true;
        backgroundPanel.SetActive(true);
        losePanel.SetActive(true);
        loseSummaryText.text = $"{points} / {goal}\nSo close!!";
    }

    public void OnWinGame() { SceneManager.LoadScene(0); }
    public void OnLoseGame() { SceneManager.LoadScene(0); }
}
