using UnityEngine;

public class FireworkSpawner : MonoBehaviour
{
    public GameObject fireworkPrefab;

    [Header("Spawn Area")]
    public Vector2 xRange = new Vector2(-4f, 4f);
    public Vector2 zRange = new Vector2(6f, 12f);
    public float spawnY = 0f;

    [Header("Timing")]
    public float minInterval = 1.5f;
    public float maxInterval = 3f;

    [Header("Count")]
    public bool autoSpawn = true;

    private float nextSpawnTime;

    void Start()
    {
        ScheduleNext();
    }

    void Update()
    {
        if (!autoSpawn || fireworkPrefab == null) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnFirework();
            ScheduleNext();
        }
    }

    public void SpawnFirework()
    {
        Vector3 pos = new Vector3(
            Random.Range(xRange.x, xRange.y),
            spawnY,
            Random.Range(zRange.x, zRange.y)
        );

        Instantiate(fireworkPrefab, pos, Quaternion.identity);
    }

    void ScheduleNext()
    {
        nextSpawnTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}