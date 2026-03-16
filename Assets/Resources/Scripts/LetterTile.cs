using UnityEngine;
using TMPro;

public class LetterTile : MonoBehaviour
{
    public string letter = "A";

    public TextMeshPro letterText;

    private Color _originalColor;
    private bool _isInvalid;
    public bool IsInvalid => _isInvalid;

    void Awake()
    {
        if (letterText != null)
            _originalColor = letterText.color;
    }

    public string GetLetter()
    {
        return letter.Trim().ToUpper();
    }

    public void SetInvalidRed()
    {
        _isInvalid = true;

        if (letterText != null)
            letterText.color = Color.red;
    }

    public void RestoreOriginalColor()
    {
        _isInvalid = false;

        if (letterText != null)
            letterText.color = _originalColor;
    }
}