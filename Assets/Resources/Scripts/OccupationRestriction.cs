using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class OccupationRestriction : MonoBehaviour
{
    public PlayerOccupation.Occupation allowedOccupation;

    private XRGrabInteractable grab;

    private void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    private void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrabAttempt);
    }

    private void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrabAttempt);
    }

    private void OnGrabAttempt(SelectEnterEventArgs args)
    {
        var interactorTransform = args.interactorObject.transform;
        var playerOccupation = FindAnyObjectByType<PlayerOccupation>();

        if (playerOccupation == null)
        {
            ForceRelease(args);
            return;
        }

        if (playerOccupation.MyOccupation != allowedOccupation)
        {
            Debug.Log($"Grab denied. Required {allowedOccupation}, but player is {playerOccupation.MyOccupation}");
            ForceRelease(args);
        }
    }

    private void ForceRelease(SelectEnterEventArgs args)
    {
        if (grab.interactionManager != null)
        {
            grab.interactionManager.SelectExit(args.interactorObject, grab);
        }
    }
}