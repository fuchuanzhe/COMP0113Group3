using UnityEngine;
using TMPro;

public class LetterTile : MonoBehaviour
{
    public string letter = "A";
    public TextMeshPro letterText;

    private Color _originalColor;
    private bool _isInvalid;
    private bool _isSceneObjectWord;

    public bool IsInvalid => _isInvalid;
    public bool IsSceneObjectWord => _isSceneObjectWord;

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
        _isSceneObjectWord = false;

        if (letterText != null)
            letterText.color = Color.red;
    }

    public void SetSceneObjectYellow()
    {
        _isInvalid = false;
        _isSceneObjectWord = true;

        if (letterText != null)
            letterText.color = Color.yellow;
    }

    public void RestoreOriginalColor()
    {
        _isInvalid = false;
        _isSceneObjectWord = false;

        if (letterText != null)
            letterText.color = _originalColor;
    }
}