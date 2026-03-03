using UnityEngine;

public class CuttableObject : MonoBehaviour
{
    [Header("Spawn Halves")]
    public GameObject halfPrefabA;
    public GameObject halfPrefabB;
    public float spawnOffset = 0.01f;
    public bool addRigidbodyIfMissing = true;
    public bool destroyOriginal = true;

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

        if (!halfPrefabA || !halfPrefabB)
        {
            Debug.LogWarning($"[CuttableObject] Missing half prefabs on {name}. Will destroy/deactivate without spawning halves.");

            LetterSpawner.Instance?.SpawnWord(word, cutCenter);

            if (destroyOriginal) Destroy(gameObject);
            else gameObject.SetActive(false);
            return;
        }

        // 让两半沿“切割面法线”分开一点点，避免重叠抖动
        Vector3 offset = planeNormal.normalized * spawnOffset;

        // 旋转：可以让 halves 面向切割面（可选）
        Quaternion rot = transform.rotation;

        var a = Instantiate(halfPrefabA, cutCenter + offset, rot);
        var b = Instantiate(halfPrefabB, cutCenter - offset, rot);

        SetupHalf(a,  initialVelocity + offset.normalized * 0.2f);
        SetupHalf(b,  initialVelocity - offset.normalized * 0.2f);

        LetterSpawner.Instance?.SpawnWord(word, cutCenter);

        if (destroyOriginal) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    void SetupHalf(GameObject go, Vector3 vel)
    {
        if (!go) return;

        var rb = go.GetComponent<Rigidbody>();
        if (!rb && addRigidbodyIfMissing) rb = go.AddComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = vel;
            rb.angularVelocity = Random.insideUnitSphere * 2f;
        }
    }
}