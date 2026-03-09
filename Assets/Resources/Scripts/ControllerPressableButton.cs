using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ControllerPressableButton : MonoBehaviour
{
    public Transform buttonVisual;
    public XRSimpleInteractable interactable;

    public Vector3 pressedLocalOffset = new Vector3(0f, -0.015f, 0f);
    public float pressDuration = 0.08f;
    public float releaseDuration = 0.12f;

    private Vector3 _startLocalPos;
    private bool _isAnimating;

    void Awake()
    {
        if (buttonVisual == null)
            buttonVisual = transform;

        if (interactable == null)
            interactable = GetComponent<XRSimpleInteractable>();

        _startLocalPos = buttonVisual.localPosition;

        if (interactable != null)
            interactable.selectEntered.AddListener(OnSelectEntered);
    }

    void OnDestroy()
    {
        if (interactable != null)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!_isAnimating)
            StartCoroutine(PressRoutine());
    }

    private IEnumerator PressRoutine()
    {
        _isAnimating = true;

        Vector3 pressedPos = _startLocalPos + pressedLocalOffset;

        float t = 0f;
        while (t < pressDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / pressDuration);
            buttonVisual.localPosition = Vector3.Lerp(_startLocalPos, pressedPos, k);
            yield return null;
        }

        t = 0f;
        while (t < releaseDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / releaseDuration);
            buttonVisual.localPosition = Vector3.Lerp(pressedPos, _startLocalPos, k);
            yield return null;
        }

        buttonVisual.localPosition = _startLocalPos;
        _isAnimating = false;
    }
}