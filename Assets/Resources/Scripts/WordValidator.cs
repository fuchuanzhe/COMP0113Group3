using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WordValidator : MonoBehaviour
{
    private HashSet<string> _dictionary = new HashSet<string>();

    void Awake()
    {
        LoadLocalDictionary();
    }

    private void LoadLocalDictionary()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "dictionary.txt");

        if (File.Exists(path))
        {
            string[] lines = File.ReadAllLines(path);
            foreach (var word in lines)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    _dictionary.Add(word.Trim().ToLower());
                }
            }
            Debug.Log($"<color=green>Dictionary Loaded!</color> Count: {_dictionary.Count}");
        }
        else
        {
            Debug.LogError($"Dictionary file not found at: {path}");
        }
    }

    
    public bool CheckWord(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        string checkMe = input.Trim().ToLower();
        return _dictionary.Contains(checkMe);
    }
}