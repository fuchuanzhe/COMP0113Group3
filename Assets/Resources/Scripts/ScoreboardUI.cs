using UnityEngine;
using TMPro;

public class ScoreboardUI : MonoBehaviour
{
    [Header("Team 1 UI")]
    public RectTransform team1FillBar;
    public TextMeshProUGUI team1ScoreText;

    [Header("Team 2 UI")]
    public RectTransform team2FillBar;
    public TextMeshProUGUI team2ScoreText;

    [Header("Bar Settings")]
    public float minBarHeight = 10f;
    public float pixelsPerPoint = 8f;
    public float maxBarHeight = 220f;

    public void SetPlayer1Score(int score)
    {
        UpdateBar(team1FillBar, team1ScoreText, score);
    }

    public void SetPlayer2Score(int score)
    {
        UpdateBar(team2FillBar, team2ScoreText, score);
    }

    private void UpdateBar(RectTransform fillBar, TextMeshProUGUI scoreText, int score)
    {
        if (fillBar != null)
        {
            Vector2 size = fillBar.sizeDelta;
            size.y = Mathf.Min(minBarHeight + score * pixelsPerPoint, maxBarHeight);
            fillBar.sizeDelta = size;
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}