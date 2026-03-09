using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SnapOnRelease : MonoBehaviour
{
    private TableSnapZone currentZone;
    private XRGrabInteractable grab;
    private BlockFootprint footprint;
    private LetterTile letterTile;

    public bool IsSnappedOnTable { get; private set; }

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        footprint = GetComponent<BlockFootprint>();
        letterTile = GetComponent<LetterTile>();

        grab.selectExited.AddListener(OnReleased);
        grab.selectEntered.AddListener(OnPicked);
    }

    void OnPicked(SelectEnterEventArgs args)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        if (currentZone) currentZone.Unmark(transform);

        IsSnappedOnTable = false;

        // 拿起来时恢复原色
        if (letterTile != null)
            letterTile.RestoreOriginalColor();
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (!currentZone) return;

        var col = GetComponentInChildren<Collider>();
        Vector3 desired = col ? col.bounds.center : transform.position;
        desired.y = currentZone.gridOrigin.position.y;

        bool ok = currentZone.TryPlace(footprint, desired, transform);
        if (!ok)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;

            IsSnappedOnTable = false;
        }
        else
        {
            IsSnappedOnTable = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var zone = other.GetComponent<TableSnapZone>();
        if (zone) currentZone = zone;
    }

    void OnTriggerExit(Collider other)
    {
        var zone = other.GetComponent<TableSnapZone>();
        if (zone && currentZone == zone) currentZone = null;
    }
}