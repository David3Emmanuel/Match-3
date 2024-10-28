using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject backgroundPanel,victoryPanel, losePanel;
    public TextMeshProUGUI pointsText, movesText;
    public int moves, goal;

    public static GameManager Instance { get; private set; }
    private int points;
    bool gameOver;

    void Awake() {Instance = this;}
    public void Initialize(int moves, int goal) {
this.moves = moves;
this.goal = goal;
    }

    public void ProcessTurn(int pointsGained, bool moveUsed) {
        points += pointsGained;
        pointsText.text = points + " / " + goal;

        if (moveUsed) {
            moves--;
            movesText.text = "Moves: " + moves;
        }

        if (points >= goal) WinGame();
        else if (moves <= 0) LoseGame();
    }

    void WinGame() {
        gameOver = true;
        backgroundPanel.SetActive(true);
        victoryPanel.SetActive(true);
    }

    void LoseGame() {
        gameOver = true;
        backgroundPanel.SetActive(true);
        losePanel.SetActive(true);
    }

    public void OnWinGame() {SceneManager.LoadScene(0);}
    public void OnLoseGame() {SceneManager.LoadScene(0);}
}
