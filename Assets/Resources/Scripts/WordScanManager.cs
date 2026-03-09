using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordScanManager : MonoBehaviour
{
    [Header("References")]
    public TableSnapZone tableZone;
    public WordValidator validator;

    [Header("Scan Settings")]
    public bool allowHorizontal = true;
    public bool allowVertical = false;
    public int minWordLength = 2;

    public ScoreManager scoreManager;
    public void ScanWords()
    {
        if (tableZone == null || validator == null)
        {
            Debug.LogWarning("WordScanManager: references not assigned.");
            return;
        }

        // find all snapped letters
        LetterTile[] allLetters = FindObjectsOfType<LetterTile>();

        Dictionary<Vector2Int, LetterTile> cellMap = new Dictionary<Vector2Int, LetterTile>();

        foreach (var letter in allLetters)
        {
            var snap = letter.GetComponent<SnapOnRelease>();
            if (snap == null || !snap.IsSnappedOnTable) continue;

            Vector2Int cell = tableZone.WorldToCell(letter.transform.position);

            if (!cellMap.ContainsKey(cell))
                cellMap.Add(cell, letter);
        }

        HashSet<LetterTile> processed = new HashSet<LetterTile>();

        if (allowHorizontal)
            ScanDirection(cellMap, processed, Vector2Int.right);

        if (allowVertical)
            ScanDirection(cellMap, processed, Vector2Int.up);
    }

    private void ScanDirection(
        Dictionary<Vector2Int, LetterTile> cellMap,
        HashSet<LetterTile> processed,
        Vector2Int dir)
    {
        foreach (var kv in cellMap)
        {
            Vector2Int cell = kv.Key;
            LetterTile startLetter = kv.Value;

            if (processed.Contains(startLetter))
                continue;

            // begin from the start of string
            Vector2Int prev = cell - dir;
            if (cellMap.ContainsKey(prev))
                continue;

            List<LetterTile> wordTiles = new List<LetterTile>();
            List<Vector2Int> wordCells = new List<Vector2Int>();

            Vector2Int current = cell;
            while (cellMap.TryGetValue(current, out LetterTile tile))
            {
                wordTiles.Add(tile);
                wordCells.Add(current);
                processed.Add(tile);
                current += dir;
            }

            if (wordTiles.Count < minWordLength)
                continue;

            string word = string.Concat(wordTiles.Select(t => t.GetLetter())).ToLower();

            if (validator.CheckWord(word))
            {
                Debug.Log($"Valid word: {word}");

                int points = word.Length;
                if (scoreManager != null)
                    scoreManager.AddPlayer1Score(points);

                foreach (var tile in wordTiles)
                {
                    if (tableZone != null)
                        tableZone.Unmark(tile.transform);

                    Destroy(tile.gameObject);
                }
            }
            else
            {
                Debug.Log($"Invalid word: {word}");

                foreach (var tile in wordTiles)
                {
                    tile.SetInvalidRed();
                }
            }
        }
    }
}