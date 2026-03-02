using UnityEngine;
using System.Collections.Generic;

public class BlockFootprint : MonoBehaviour
{
    // one letter only occpuies one cell
    public List<Vector2Int> Cells = new() { Vector2Int.zero };
    public Transform forwardRef;

    [HideInInspector] public Vector2 worldSizeXZ;     // world scale
    [HideInInspector] public Vector3 WorldHalfExtents; // for overlapping detection

    public void RecalculateFromCollider()
    {
        var col = GetComponentInChildren<Collider>();
        if (!col) return;

        var size = col.bounds.size;
        worldSizeXZ = new Vector2(size.x, size.z);
        WorldHalfExtents = col.bounds.extents;
    }

    void Awake() => RecalculateFromCollider();
    void OnValidate() => RecalculateFromCollider();
}