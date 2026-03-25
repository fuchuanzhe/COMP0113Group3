using UnityEngine;
using System.Text.RegularExpressions;

public class SmashableObject : MonoBehaviour
{
    public bool IsSmashed { get; private set; }

    public string word { get; private set; }

    void Awake()
    {
        // Extract uppercase letters from object name
        word = Regex.Replace(gameObject.name.ToUpperInvariant(), "[^A-Z]", "");
    }

    public void DoSmash(Vector3 hitPoint)
    {
        if (IsSmashed) return;
        IsSmashed = true;

        LetterSpawner.Instance?.SpawnWord(word, hitPoint);
        var networkObj = GetComponent<NetworkedObject>();
        if (networkObj != null)
            networkObj.BroadcastActiveSelf(false);

        Destroy(gameObject);
    }
}
