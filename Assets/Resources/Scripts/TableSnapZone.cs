using System.Collections.Generic;
using UnityEngine;

public class TableSnapZone : MonoBehaviour
{
    [Header("Grid")]
    public Transform gridOrigin;
    public Vector2 cellSize = new Vector2(0.1f, 0.1f);   // X,Z
    public Vector2Int gridSize = new Vector2Int(10, 10); // 10x10, adjust this in the inspector
    public float snapY = 0.0f;                           // relative to Grid Origin
    public int searchRadiusCells = 4;

    [Header("Collision Check")]
    public LayerMask blockLayerMask;
    public float overlapShrink = 0.98f;

    private readonly Dictionary<Vector2Int, Transform> occupied = new();

    [Header("Debug Gizmos")]
    public bool drawGridGizmos = true;
    public bool drawCellCenters = true;
    public float centerDotRadius = 0.01f;
    public bool drawLastPlacement = true;

    private bool _hasLast;
    private Vector3 _lastDesiredWorld;
    private Vector2Int _lastDesiredCell;
    private Vector2Int _lastChosenCell;
    private Pose _lastChosenPose;

    public bool TryPlace(BlockFootprint footprint, Vector3 desiredWorldPoint, Transform objectRoot)
    {
        _hasLast = true;    // released position
        _lastDesiredWorld = desiredWorldPoint;
        _lastDesiredCell = WorldToCell(desiredWorldPoint);

        Vector2Int desiredCell = _lastDesiredCell;

        if (!FindNearestValidCell(footprint, desiredCell, out Vector2Int chosenCell))
            return false;

        Pose pose = CellToPose(chosenCell, footprint);

        _lastChosenCell = chosenCell;
        _lastChosenPose = pose;

        if (!PassOverlapCheck(footprint, pose, objectRoot))
            return false;

        ApplyPlacement(objectRoot, pose);
        MarkOccupied(footprint, chosenCell, objectRoot);
        return true;
    }

    Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = gridOrigin.InverseTransformPoint(world);

        int cx = Mathf.FloorToInt(local.x / cellSize.x);
        int cz = Mathf.FloorToInt(local.z / cellSize.y);

        int x = cx + gridSize.x / 2;
        int z = cz + gridSize.y / 2;

        return new Vector2Int(x, z);
    }

    Pose CellToPose(Vector2Int cell, BlockFootprint footprint)
    {
        int cx = cell.x - gridSize.x / 2;
        int cz = cell.y - gridSize.y / 2;

        Vector3 localPos = new Vector3((cx + 0.5f) * cellSize.x, snapY, (cz + 0.5f) * cellSize.y);
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

    // Visualize the grid
    private void OnDrawGizmosSelected()
    {
        if (!drawGridGizmos || gridOrigin == null) return;

        float yLift = 0.002f;

        int halfX = gridSize.x / 2;
        int halfZ = gridSize.y / 2;

        Vector3 OriginToWorld(float lx, float lz)
        {
            Vector3 local = new Vector3(lx, snapY, lz);
            Vector3 w = gridOrigin.TransformPoint(local);
            w.y = gridOrigin.position.y + snapY + yLift;
            return w;
        }

        // draw boundaries
        for (int ix = 0; ix <= gridSize.x; ix++)
        {
            int cx = ix - halfX;
            float lx = cx * cellSize.x;
            float lz0 = (-halfZ) * cellSize.y;
            float lz1 = (gridSize.y - halfZ) * cellSize.y;

            Gizmos.DrawLine(OriginToWorld(lx, lz0), OriginToWorld(lx, lz1));
        }

        for (int iz = 0; iz <= gridSize.y; iz++)
        {
            int cz = iz - halfZ;
            float lz = cz * cellSize.y;
            float lx0 = (-halfX) * cellSize.x;
            float lx1 = (gridSize.x - halfX) * cellSize.x;

            Gizmos.DrawLine(OriginToWorld(lx0, lz), OriginToWorld(lx1, lz));
        }

        // draw centre points
        if (drawCellCenters)
        {
            for (int x = 0; x < gridSize.x; x++)
                for (int z = 0; z < gridSize.y; z++)
                {
                    int cx = x - halfX;
                    int cz = z - halfZ;

                    float centerX = (cx + 0.5f) * cellSize.x;
                    float centerZ = (cz + 0.5f) * cellSize.y;

                    Vector3 p = OriginToWorld(centerX, centerZ);
                    Gizmos.DrawCube(p, Vector3.one * centerDotRadius);
                }
        }

        // draw desired / chosen
        if (drawLastPlacement && _hasLast)
        {
            Gizmos.DrawSphere(_lastDesiredWorld, centerDotRadius * 1.2f);
            Gizmos.DrawSphere(_lastChosenPose.position, centerDotRadius * 1.8f);
            Gizmos.DrawLine(_lastDesiredWorld, _lastChosenPose.position);
        }
    }

    

}