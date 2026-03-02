using System.Collections.Generic;
using UnityEngine;

public class BlockFootprint : MonoBehaviour
{
    public List<Vector2Int> Cells = new List<Vector2Int> { Vector2Int.zero };

    public Vector3 WorldHalfExtents = new Vector3(0.05f, 0.05f, 0.05f);
    public Transform forwardRef;

    public float CurrentYawDeg
    {
        get
        {
            var e = transform.rotation.eulerAngles;
            return e.y;
        }
    }

    void Awake()
    {
        var col = GetComponentInChildren<Collider>();
        if (col) WorldHalfExtents = col.bounds.extents;
    }
}