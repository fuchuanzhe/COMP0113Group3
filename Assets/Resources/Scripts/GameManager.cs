using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public ScoreManager scoreManager;
    public FireworkSpawner player1FireworkSpawner;
    public FireworkSpawner player2FireworkSpawner;

    [Header("Win Conditions")]
    public float matchDuration = 300f;   // 5 minutes
    public int targetScore = 100;

    [Header("UI (Optional)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText;

    private float remainingTime;
    private bool gameEnded = false;

    void Start()
    {
        remainingTime = matchDuration;

        if (resultText != null)
            resultText.text = "";

        // 开局先不自动放烟花
        if (player1FireworkSpawner != null)
            player1FireworkSpawner.autoSpawn = false;

        if (player2FireworkSpawner != null)
            player2FireworkSpawner.autoSpawn = false;
    }

    void Update()
    {
        if (gameEnded) return;

        // 1. timer countdown
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0f)
            remainingTime = 0f;

        UpdateTimerUI();

        // 2. score check
        int p1 = scoreManager != null ? scoreManager.GetPlayer1Score() : 0;
        int p2 = scoreManager != null ? scoreManager.GetPlayer2Score() : 0;

        if (p1 >= targetScore || p2 >= targetScore)
        {
            EndGame();
            return;
        }

        // 3. time up
        if (remainingTime <= 0f)
        {
            EndGame();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        int p1 = scoreManager != null ? scoreManager.GetPlayer1Score() : 0;
        int p2 = scoreManager != null ? scoreManager.GetPlayer2Score() : 0;

        Debug.Log($"Game Over! Player1={p1}, Player2={p2}");

        // 判定赢家
        if (p1 > p2)
        {
            Debug.Log("Player 1 wins!");
            if (resultText != null) resultText.text = "Player 1 Wins!";
            PlayWinnerFireworks(1);
        }
        else if (p2 > p1)
        {
            Debug.Log("Player 2 wins!");
            if (resultText != null) resultText.text = "Player 2 Wins!";
            PlayWinnerFireworks(2);
        }
        else
        {
            Debug.Log("Draw!");
            if (resultText != null) resultText.text = "Draw!";
        }

        // 这里先只“结束计分逻辑”
        // 如果你后面想彻底禁止继续拼词，我们下一步再加
    }

    void PlayWinnerFireworks(int winner)
    {
        if (winner == 1 && player1FireworkSpawner != null)
        {
            player1FireworkSpawner.autoSpawn = true;
        }
        else if (winner == 2 && player2FireworkSpawner != null)
        {
            player2FireworkSpawner.autoSpawn = true;
        }
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }
}