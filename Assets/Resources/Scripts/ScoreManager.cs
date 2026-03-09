using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int player1Score = 0;
    public int player2Score = 0;

    public ScoreboardUI scoreboardUI;

    void Start()
    {
        RefreshUI();
    }

    public void AddPlayer1Score(int amount)
    {
        player1Score += amount;
        if (player1Score < 0) player1Score = 0;

        RefreshUI();
    }

    public void AddPlayer2Score(int amount)
    {
        player2Score += amount;
        if (player2Score < 0) player2Score = 0;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (scoreboardUI == null) return;

        scoreboardUI.SetPlayer1Score(player1Score);
        scoreboardUI.SetPlayer2Score(player2Score);
    }
}