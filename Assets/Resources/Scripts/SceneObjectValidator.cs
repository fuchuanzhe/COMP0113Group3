using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SceneObjectValidator : MonoBehaviour
{
    private HashSet<string> _dictionary = new HashSet<string>();

    void Awake()
    {
        StartCoroutine(LoadSceneObjectDictionary());
    }

    IEnumerator LoadSceneObjectDictionary()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "scene_objects.txt");

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
                    _dictionary.Add(word.Trim().ToLower());
                }
            }
        }
    }

    public bool CheckWord(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;
        string checkMe = input.Trim().ToLower();
        return _dictionary.Contains(checkMe);
    }
}