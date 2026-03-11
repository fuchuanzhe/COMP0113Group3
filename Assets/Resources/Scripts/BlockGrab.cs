using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BlockGrab : MonoBehaviour
{
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
        ForceRelease(args);
    }

    private void ForceRelease(SelectEnterEventArgs args)
    {
        if (grab.interactionManager != null)
        {
            grab.interactionManager.SelectExit(args.interactorObject, grab);
        }
    }
}
