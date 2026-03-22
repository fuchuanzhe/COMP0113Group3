using UnityEngine;
using TMPro;
using Ubiq.Messaging;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public ScoreManager scoreManager;
    public FireworkSpawner player1FireworkSpawner;
    public FireworkSpawner player2FireworkSpawner;
    private bool matchStarted = false;

    [Header("Win Conditions")]
    public float matchDuration = 300f;   // 5 minutes
    public int targetScore = 100;

    [Header("UI (Optional)")]
    public TextMeshProUGUI timerTextTeam1;
    public TextMeshProUGUI timerTextTeam2;
    public TextMeshProUGUI resultText;

    private float remainingTime;
    private bool gameEnded = false;
    public NetworkContext context;

    private struct Message
    {
        public bool startMatch;
        public bool endMatch;
        public int winner;
        public float remainingTime;

        public Message(bool startMatch, bool endMatch, int winner, float remainingTime)
        {
            this.startMatch = startMatch;
            this.endMatch = endMatch;
            this.winner = winner;
            this.remainingTime = remainingTime;
        }
    }
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();

        if (m.startMatch)
        {
            if (!matchStarted && !gameEnded)
            {
                remainingTime = m.remainingTime;
                matchStarted = true;
                UpdateTimerUI();
                Debug.Log("Match started from network message.");
            }
        }

        if (m.endMatch)
        {
            if (!gameEnded)
            {
                remainingTime = m.remainingTime;
                matchStarted = false;
                gameEnded = true;
                UpdateTimerUI();

                ApplyWinnerResult(m.winner);
                Debug.Log($"Match ended from network message. Winner = {m.winner}");
            }
        }
    }

    void Start()
    {
        remainingTime = matchDuration;
        matchStarted = false;
        gameEnded = false;
        context = NetworkScene.Register(this);

        UpdateTimerUI();

        if (player1FireworkSpawner != null)
            player1FireworkSpawner.autoSpawn = false;

        if (player2FireworkSpawner != null)
            player2FireworkSpawner.autoSpawn = false;
    }

    void Update()
    {
        if (gameEnded) return;
        if (!matchStarted) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime < 0f)
            remainingTime = 0f;

        UpdateTimerUI();

        int p1 = scoreManager != null ? scoreManager.GetPlayer1Score() : 0;
        int p2 = scoreManager != null ? scoreManager.GetPlayer2Score() : 0;

        if (p1 >= targetScore || p2 >= targetScore)
        {
            EndGame();
            return;
        }

        if (remainingTime <= 0f)
        {
            EndGame();
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        string timeString = $"Time: {minutes:00}:{seconds:00}";

        if (timerTextTeam1 != null)
            timerTextTeam1.text = timeString;

        if (timerTextTeam2 != null)
            timerTextTeam2.text = timeString;
    }

    void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        matchStarted = false;

        int p1 = scoreManager != null ? scoreManager.GetPlayer1Score() : 0;
        int p2 = scoreManager != null ? scoreManager.GetPlayer2Score() : 0;

        Debug.Log($"Game Over! Player1={p1}, Player2={p2}");

        int winner = 0;
        if (p1 > p2) winner = 1;
        else if (p2 > p1) winner = 2;

        ApplyWinnerResult(winner);

        context.SendJson(new Message(false, true, winner, remainingTime));
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
    public void BeginMatch()
    {
        if (gameEnded) return;
        if (matchStarted) return;

        remainingTime = matchDuration;
        matchStarted = true;
        UpdateTimerUI();

        Debug.Log("Match started!");

        context.SendJson(new Message(true, false, 0, remainingTime));
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }
    void ApplyWinnerResult(int winner)
    {
        if (winner == 1)
        {
            Debug.Log("Player 1 wins!");
            if (resultText != null) resultText.text = "Player 1 Wins!";
            PlayWinnerFireworks(1);
        }
        else if (winner == 2)
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
    }
}