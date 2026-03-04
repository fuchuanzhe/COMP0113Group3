using UnityEngine;

public class SmashableObject : MonoBehaviour
{
    [Header("Smash Spawn")]
    public GameObject smashedPrefab;       // 砸碎后的整体（带碎片rigidbody）或碎片父物体
    public bool destroyOriginal = true;

    [Header("Impulse")]
    public bool addRigidbodyIfMissing = false; // 如果碎片没RB，是否自动加（一般碎片prefab里自己配好）
    public float randomAngular = 6f;

    public bool IsSmashed { get; private set; }

    public string word { get; private set; }

    void Awake()
    {
        word = gameObject.name.ToUpperInvariant();
    }

    public void DoSmash(Vector3 hitPoint, Vector3 impulse)
    {
        if (IsSmashed) return;
        IsSmashed = true;

        if (!smashedPrefab)
        {
            Debug.LogWarning($"[SmashableObject] Missing smashedPrefab on {name}. Will destroy/deactivate without spawning smashed prefab.");
        }
        else
        {
            var go = Instantiate(smashedPrefab, transform.position, transform.rotation);

            // 给 prefab 内所有碎片一点冲量（如果 prefab 是一个父物体带很多子碎片）
            var rbs = go.GetComponentsInChildren<Rigidbody>(true);
            if (rbs.Length == 0 && addRigidbodyIfMissing)
            {
                // 退化：至少给根加一个
                var rb = go.AddComponent<Rigidbody>();
                rb.AddForce(impulse, ForceMode.VelocityChange);
                rb.angularVelocity = Random.insideUnitSphere * randomAngular;
            }
            else
            {
                for (int i = 0; i < rbs.Length; i++)
                {
                    var rb = rbs[i];
                    if (!rb) continue;

                    // 冲量随距离稍微衰减（更自然）
                    float dist = Vector3.Distance(hitPoint, rb.worldCenterOfMass);
                    float falloff = 1f / (1f + dist * 3f);

                    rb.AddForce(impulse * falloff, ForceMode.VelocityChange);
                    rb.angularVelocity += Random.insideUnitSphere * randomAngular;
                }
            }
        }

        LetterSpawner.Instance?.SpawnWord(word, hitPoint);
        var networkObj = GetComponent<DualNetworkedObject>();
        if (networkObj != null)
            networkObj.BroadcastActiveSelf(false);

        if (destroyOriginal) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}