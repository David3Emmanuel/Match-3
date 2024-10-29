using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject backgroundPanel, victoryPanel, losePanel;
    public TextMeshProUGUI pointsText, movesText, winSummaryText, loseSummaryText;
    public Level[] levels;

    public Level CurrentLevel { get; private set; }
    public static GameManager Instance { get; private set; }
    public bool GameOver
    {
        get { return gameOver; }
        private set
        {
            if (value)
            {
                AudioManager.Instance.StopMusic();
                PotionBoard.Instance.gameObject.SetActive(false);
                backgroundPanel.SetActive(true);
            }
            gameOver = value;
        }
    }

    private int points, moves;
    private bool gameOver;


    void Awake()
    {
        Instance = this;
        CurrentLevel = levels[PlayerPrefs.GetInt("Level", 0)];
    }

    void Start()
    {
        AudioManager.Instance.PlayLevelMusic();


        moves = CurrentLevel.moves;
        movesText.text = "Moves: " + moves;
        pointsText.text = points + " / " + CurrentLevel.goal;
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
        pointsText.text = points + " / " + CurrentLevel.goal;

        if (points >= CurrentLevel.goal) StartCoroutine(WinGame());
    }

    IEnumerator WinGame()
    {
        while (PotionBoard.Instance.IsProcessing)
            yield return null;

        GameOver = true;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.winSFX);
        victoryPanel.SetActive(true);
        winSummaryText.text = $"You won in {moves} moves";
    }

    IEnumerator LoseGame()
    {
        while (PotionBoard.Instance.IsProcessing)
            yield return null;

        GameOver = true;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.loseSFX);
        losePanel.SetActive(true);
        loseSummaryText.text = $"{points} / {CurrentLevel.goal}\nSo close!!";
    }

    public void OnWinGame()
    {
        int nextLevel = PlayerPrefs.GetInt("Level", 0) + 1;
        if (nextLevel < levels.Length)
            PlayerPrefs.SetInt("Level", nextLevel);
        else
            PlayerPrefs.SetInt("Level", 0);

        AudioManager.Instance.PlaySFX(AudioManager.Instance.clickSFX);
        SceneManager.LoadScene(0);
    }

    public void OnLoseGame()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.clickSFX);
        SceneManager.LoadScene(0);
    }
}
