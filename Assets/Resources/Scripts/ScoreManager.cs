using UnityEngine;
using Ubiq.Messaging;

public class ScoreManager : MonoBehaviour
{
    private NetworkContext context;
    private struct Message
    {
        public int player1Score;
        public int player2Score;

        public Message(int player1Score, int player2Score)
        {
            this.player1Score = player1Score;
            this.player2Score = player2Score;
        }
    }

    public int player1Score = 0;
    public int player2Score = 0;

    public ScoreboardUI scoreboardUI;
    public ScoreboardUI scoreboardUI2;

    void Start()
    {
        context = NetworkScene.Register(this);
        RefreshUI();
    }

    public void AddPlayer1Score(int amount)
    {
        player1Score += amount;
        if (player1Score < 0) player1Score = 0;

        RefreshUI();
        context.SendJson(new Message(player1Score, player2Score));
    }

    public void AddPlayer2Score(int amount)
    {
        player2Score += amount;
        if (player2Score < 0) player2Score = 0;

        RefreshUI();
        context.SendJson(new Message(player1Score, player2Score));
    }

    public void RefreshUI()
    {
        if (scoreboardUI == null || scoreboardUI2 == null) return;

        scoreboardUI.SetPlayer1Score(player1Score);
        scoreboardUI.SetPlayer2Score(player2Score);

        scoreboardUI2.SetPlayer1Score(player1Score);
        scoreboardUI2.SetPlayer2Score(player2Score);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        player1Score = m.player1Score;
        player2Score = m.player2Score;
        RefreshUI();
    }
}