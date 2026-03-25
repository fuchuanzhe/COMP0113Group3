using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class WordValidator : MonoBehaviour
{
    // Store words for fast O(1) lookups
    private HashSet<string> _dictionary = new HashSet<string>();

    void Awake()
    {
        // Load dictionary asynchronously on startup
        StartCoroutine(LoadLocalDictionary());
    }

    IEnumerator LoadLocalDictionary()
    {
        // Build path to the dictionary file
        string path = Path.Combine(Application.streamingAssetsPath, "dictionary.txt");

        UnityWebRequest www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string text = www.downloadHandler.text;
            string[] lines = text.Split('\n');

            foreach (var word in lines)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    // Add cleaned, lowercase word to the set
                    _dictionary.Add(word.Trim().ToLower());
                }
            }

        }
    }

    // Check if the input word is valid
    public bool CheckWord(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        
        // Format input for accurate comparison
        string checkMe = input.Trim().ToLower();
        
        return _dictionary.Contains(checkMe);
    }
}