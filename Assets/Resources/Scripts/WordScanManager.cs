using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WordScanManager : MonoBehaviour
{
    [Header("References")]
    public TableSnapZone tableZone;
    public WordValidator validator;
    public ScoreManager scoreManager;

    [Header("Scan Settings")]
    public bool allowHorizontal = true;
    public bool allowVertical = false;
    public int minWordLength = 2;

    public void ScanWords()
    {
        if (tableZone == null || validator == null)
        {
            Debug.LogWarning("WordScanManager: references not assigned.");
            return;
        }

        LetterTile[] allLetters = FindObjectsOfType<LetterTile>();

        foreach (var letter in allLetters)
        {
            letter.RestoreOriginalColor();
        }

        Dictionary<Vector2Int, LetterTile> cellMap = new Dictionary<Vector2Int, LetterTile>();

        foreach (var letter in allLetters)
        {
            if (letter == null) continue;

            Vector2Int cell = tableZone.WorldToCell(letter.transform.position);

            if (!IsInsideGrid(cell))
                continue;

            if (!cellMap.ContainsKey(cell))
            {
                cellMap.Add(cell, letter);
            }
            else
            {
                Debug.LogWarning($"Duplicate letters found in same cell {cell}, ignoring extra one: {letter.name}");
            }
        }

        Debug.Log($"[WordScanManager] Letters inside grid: {cellMap.Count}");

        HashSet<LetterTile> processed = new HashSet<LetterTile>();

        if (allowHorizontal)
            ScanDirection(cellMap, processed, Vector2Int.right);

        if (allowVertical)
            ScanDirection(cellMap, processed, Vector2Int.up);
    }

    private bool IsInsideGrid(Vector2Int cell)
    {
        return cell.x >= 0 &&
               cell.y >= 0 &&
               cell.x < tableZone.gridSize.x &&
               cell.y < tableZone.gridSize.y;
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
                Debug.Log($"<color=green>Valid word:</color> {word}");

                int points = word.Length;
                if (scoreManager != null)
                    scoreManager.AddPlayer1Score(points);

                foreach (var tile in wordTiles)
                {
                    if (tableZone != null)
                        tableZone.Unmark(tile.transform);

                    // networking
                    var spawnObj = tile.gameObject.GetComponent<SpawnableObject>();
                    spawnObj.BroadcastActiveSelf(false);
                    tile.gameObject.SetActive(false);
                    Destroy(tile.gameObject);
                }
            }
            else
            {
                Debug.Log($"<color=red>Invalid word:</color> {word}");

                foreach (var tile in wordTiles)
                {
                    tile.SetInvalidRed();
                }
            }
        }
    }
}