using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SnapOnRelease : MonoBehaviour
{
    private TableSnapZone currentZone;
    private XRGrabInteractable grab;
    private BlockFootprint footprint;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        footprint = GetComponent<BlockFootprint>();

        grab.selectExited.AddListener(OnReleased);
        grab.selectEntered.AddListener(OnPicked);
    }

    void OnPicked(SelectEnterEventArgs args)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        if (currentZone) currentZone.Unmark(transform); // clear the occupation
    }

    void OnReleased(SelectExitEventArgs args)
    {
        if (!currentZone) return;

        var col = GetComponentInChildren<Collider>();
        Vector3 desired = col ? col.bounds.center : transform.position;
        desired.y = currentZone.gridOrigin.position.y; // only x and z

        bool ok = currentZone.TryPlace(footprint, desired, transform);
        if (!ok)
        {
            // if the table is full, not snapped
            var rb = GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = false;
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