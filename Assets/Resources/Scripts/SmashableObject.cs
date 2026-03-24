using UnityEngine;

public class SmashableObject : MonoBehaviour
{
    public bool IsSmashed { get; private set; }

    public string word { get; private set; }

    void Awake()
    {
        word = gameObject.name.ToUpperInvariant();
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
