using System.Collections.Generic;
using UnityEngine;
using Ubiq.Spawning;
using System.Collections;
using System;
using Ubiq.Messaging;

public class LetterSpawner : MonoBehaviour
{
    public static LetterSpawner Instance { get; private set; }
    public PrefabCatalogue letterPrefabs;

    public float letterSpacing = 0.06f;
    public bool centerAlign = true;
    public float liftY = 0.02f;

    public bool faceCameraYawOnly = true;
    public Transform cameraTransform;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (!cameraTransform && Camera.main)
            cameraTransform = Camera.main.transform;
    }

    public void SpawnWord(string word, Vector3 spawnPos)
    {
        if (string.IsNullOrEmpty(word)) return;

        word = word.ToUpperInvariant();
        int n = word.Length;

        float total = (n - 1) * letterSpacing;
        float left = centerAlign ? -total * 0.5f : 0f;

        Quaternion rot = Quaternion.identity;

        if (cameraTransform)
        {
            Vector3 toCam = cameraTransform.position - spawnPos;
            toCam.y = 0f;

            if (toCam.sqrMagnitude < 1e-6f)
                rot = Quaternion.identity;
            else
                rot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        }

        rot *= Quaternion.Euler(0f, 180f, 0f);

        Vector3 basePos = spawnPos + Vector3.up * liftY;

        for (int i = 0; i < n; i++)
        {
            int index = word[i] - 'A';

            if (index < 0 || index >= letterPrefabs.prefabs.Count || !letterPrefabs.prefabs[index])
            {
                Debug.LogWarning($"Missing prefab for {word[i]}");
                continue;
            }

            Vector3 offset = rot * new Vector3(left + i * letterSpacing, 0f, 0f);
            var letter = NetworkSpawnManager.Find(this).SpawnWithPeerScope(letterPrefabs.prefabs[index]); 
            
            letter.transform.position = basePos + offset;
            letter.transform.rotation = rot;
            StartCoroutine(BroadcastNextFrame(letter.GetComponent<SpawnableObject>()));
        }
    }

    IEnumerator BroadcastNextFrame(SpawnableObject obj)
    {
        while(obj.context.Scene == null)
        {
            yield return null;
        }
        yield return null;
        yield return null;
        obj.BroadcastPosAndRot();
    }
}
