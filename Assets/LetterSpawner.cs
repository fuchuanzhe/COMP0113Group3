using System.Collections.Generic;
using UnityEngine;

public class LetterSpawner : MonoBehaviour
{
    [Header("Resources Path")]
    public string resourcesFolder = "Letters"; // Assets/Resources/Letters

    [Header("Layout")]
    public float letterSpacing = 0.06f;
    public bool centerAlign = true;
    public float liftY = 0.02f;

    [Header("Rotation")]
    public bool faceCameraYawOnly = true;
    public Transform cameraTransform;

    // Record handed TearObjects
    private readonly HashSet<int> _handled = new HashSet<int>();

    void Awake()
    {
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        var tearObjects = FindObjectsByType<TearObject>(
        FindObjectsInactive.Include,
        FindObjectsSortMode.None
        );

        foreach (var t in tearObjects)
        {
            if (t == null) continue;
            int id = t.GetInstanceID();
            if (_handled.Contains(id)) continue;
            if (t.isTeared)
            {
                _handled.Add(id);
                SpawnWord(t.word, t.tearCenter, Quaternion.identity);
            }
        }
    }

    public void SpawnWord(string word, Vector3 spawnPos, Quaternion baseRotation)
    {
        if (string.IsNullOrWhiteSpace(word)) return;
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;

        string upper = word.ToUpperInvariant();
        int n = upper.Length;

        float total = (n - 1) * letterSpacing;
        float left = centerAlign ? -total * 0.5f : 0f;

        Quaternion rot = baseRotation;
        if (cameraTransform && faceCameraYawOnly)
        {
            Vector3 toCam = cameraTransform.position - spawnPos;
            toCam.y = 0f;
            if (toCam.sqrMagnitude > 1e-6f)
                rot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        }
        rot = rot * Quaternion.Euler(90f, 0f, 0f);
        Vector3 basePos = spawnPos + Vector3.up * liftY;

        for (int i = 0; i < n; i++)
        {
            char c = upper[i];
            if (c < 'A' || c > 'Z') continue;
            string path = $"{resourcesFolder}/{c}";
            var prefab = Resources.Load<GameObject>(path);
            if (!prefab)
            {
                Debug.LogWarning($"[LetterSpawner] Missing prefab at Resources/{path}");
                continue;
            }

            Vector3 offset = rot * new Vector3(left + i * letterSpacing, 0f, 0f);
            Instantiate(prefab, basePos + offset, rot);
        }
    }
}