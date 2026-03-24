using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    public GameObject halfPrefabA;
    public GameObject halfPrefabB;
    public float spawnOffset = 0.01f;

    public bool IsCut { get; private set; }
    public string word { get; private set; }

    void Awake()
    {
        word = gameObject.name.ToUpperInvariant();
    }

    public void DoCut(Vector3 cutCenter, Vector3 planeNormal, Vector3 initialVelocity)
    {
        if (IsCut) return;
        IsCut = true;
        var networkObj = GetComponent<NetworkedObject>();

        if (!halfPrefabA || !halfPrefabB)
        {
            LetterSpawner.Instance?.SpawnWord(word, cutCenter);
            if (networkObj != null)
            networkObj.BroadcastActiveSelf(false);

            Destroy(gameObject);
            return;
        }

        Vector3 offset = planeNormal.normalized * spawnOffset;
        Quaternion rot = transform.rotation;

        var a = Instantiate(halfPrefabA, cutCenter + offset, rot);
        var b = Instantiate(halfPrefabB, cutCenter - offset, rot);

        SetupHalf(a,  initialVelocity + offset.normalized * 0.2f);
        SetupHalf(b,  initialVelocity - offset.normalized * 0.2f);

        LetterSpawner.Instance?.SpawnWord(word, cutCenter);
        if (networkObj != null)
            networkObj.BroadcastActiveSelf(false);

        Destroy(gameObject);
    }

    void SetupHalf(GameObject go, Vector3 vel)
    {
        if (!go) return;

        var rb = go.GetComponent<Rigidbody>();
        if (!rb) rb = go.AddComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = vel;
            rb.angularVelocity = Random.insideUnitSphere * 2f;
        }
    }
}
