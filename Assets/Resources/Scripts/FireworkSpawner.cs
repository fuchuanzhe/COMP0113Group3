using UnityEngine;

public class FireworkSpawner : MonoBehaviour
{
    public GameObject fireworkPrefab;

    public Vector2 xRange = new Vector2(-4f, 4f);
    public Vector2 zRange = new Vector2(6f, 12f);
    public float spawnY = 0f;

    public float minInterval = 1.5f;
    public float maxInterval = 3f;

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
        Vector3 pos = transform.position + new Vector3(
            Random.Range(xRange.x, xRange.y),
            spawnY,
            Random.Range(zRange.x, zRange.y)
        );

        Debug.Log($"Spawn firework at {pos}");
        Instantiate(fireworkPrefab, pos, Quaternion.identity);
    }

    void ScheduleNext()
    {
        nextSpawnTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}