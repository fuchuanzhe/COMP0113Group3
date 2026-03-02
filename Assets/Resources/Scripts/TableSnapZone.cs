using System.Collections.Generic;
using UnityEngine;

public class TableSnapZone : MonoBehaviour
{
    [Header("Grid")]
    public Transform gridOrigin;
    public Vector2 cellSize = new Vector2(0.1f, 0.1f);   // X,Z
    public Vector2Int gridSize = new Vector2Int(10, 10); // 10x10
    public float snapY = 0.0f;                           // relative to Grid Origin
    public int searchRadiusCells = 4;

    [Header("Collision Check")]
    public LayerMask blockLayerMask;
    public float overlapShrink = 0.98f;

    private readonly Dictionary<Vector2Int, Transform> occupied = new();

    public bool TryPlace(BlockFootprint footprint, Vector3 desiredWorldPoint, Transform objectRoot)
    {
        Vector2Int desiredCell = WorldToCell(desiredWorldPoint);

        if (!FindNearestValidCell(footprint, desiredCell, out Vector2Int chosenCell))
            return false;

        Pose pose = CellToPose(chosenCell, footprint);

        if (!PassOverlapCheck(footprint, pose, objectRoot))
            return false;

        ApplyPlacement(objectRoot, pose);
        MarkOccupied(footprint, chosenCell, objectRoot);
        return true;
    }

    Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = gridOrigin.InverseTransformPoint(world);
        int x = Mathf.RoundToInt(local.x / cellSize.x);
        int z = Mathf.RoundToInt(local.z / cellSize.y);
        return new Vector2Int(x, z);
    }

    Pose CellToPose(Vector2Int cell, BlockFootprint footprint)
    {
        Vector3 localPos = new Vector3(cell.x * cellSize.x, snapY, cell.y * cellSize.y);
        Vector3 worldPos = gridOrigin.TransformPoint(localPos);

        Quaternion rot;

        Vector3 fwd = Vector3.ProjectOnPlane(gridOrigin.forward, Vector3.up).normalized;
        if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
        Quaternion target = Quaternion.LookRotation(fwd, Vector3.up);

        if (footprint.forwardRef != null)
        {
            rot = target * Quaternion.Inverse(footprint.forwardRef.localRotation);
        }
        else
        {
            rot = target; 
        }
        return new Pose(worldPos, rot);
    }

    bool FindNearestValidCell(BlockFootprint footprint, Vector2Int desired, out Vector2Int chosen)
    {
        if (IsCellValidForFootprint(footprint, desired))
        {
            chosen = desired;
            return true;
        }

        for (int r = 1; r <= searchRadiusCells; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                var c1 = new Vector2Int(desired.x + dx, desired.y - r);
                if (IsCellValidForFootprint(footprint, c1)) { chosen = c1; return true; }

                var c2 = new Vector2Int(desired.x + dx, desired.y + r);
                if (IsCellValidForFootprint(footprint, c2)) { chosen = c2; return true; }
            }

            for (int dz = -r + 1; dz <= r - 1; dz++)
            {
                var c1 = new Vector2Int(desired.x - r, desired.y + dz);
                if (IsCellValidForFootprint(footprint, c1)) { chosen = c1; return true; }

                var c2 = new Vector2Int(desired.x + r, desired.y + dz);
                if (IsCellValidForFootprint(footprint, c2)) { chosen = c2; return true; }
            }
        }

        chosen = default;
        return false;
    }

    bool IsCellValidForFootprint(BlockFootprint footprint, Vector2Int anchorCell)
    {
        foreach (var offset in footprint.Cells)
        {
            Vector2Int c = anchorCell + offset;
            if (c.x < 0 || c.y < 0 || c.x >= gridSize.x || c.y >= gridSize.y) return false;
            if (occupied.ContainsKey(c)) return false;
        }
        return true;
    }

    bool PassOverlapCheck(BlockFootprint footprint, Pose pose, Transform objectRoot)
    {
        Vector3 halfExtents = footprint.WorldHalfExtents * overlapShrink;

        Collider[] hits = Physics.OverlapBox(
            pose.position,
            halfExtents,
            pose.rotation,
            blockLayerMask,
            QueryTriggerInteraction.Ignore
        );

        foreach (var h in hits)
        {
            if (!h) continue;
            if (h.transform.root == objectRoot) continue;
            return false;
        }
        return true;
    }

    void ApplyPlacement(Transform root, Pose pose)
    {
        root.SetPositionAndRotation(pose.position, pose.rotation);

        var rb = root.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    void MarkOccupied(BlockFootprint footprint, Vector2Int anchorCell, Transform root)
    {
        foreach (var offset in footprint.Cells)
            occupied[anchorCell + offset] = root;
    }

    public void Unmark(Transform root)
    {
        var toRemove = new List<Vector2Int>();
        foreach (var kv in occupied)
            if (kv.Value == root) toRemove.Add(kv.Key);
        foreach (var k in toRemove) occupied.Remove(k);
    }
}