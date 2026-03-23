using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TearColliderSwitcher : MonoBehaviour
{
    [Header("References")]
    public XRGrabInteractable grab;
    public MeshCollider tearMeshCollider;
    public Collider[] normalColliders;

    [Header("Modes")]
    [Tooltip("When grabbed, keep normal colliders enabled. Turn this off if you only want mesh(trigger) while grabbing.")]
    public bool keepNormalCollidersWhenGrabbed = true;

    [Tooltip("Enable mesh collider while grabbed, as trigger only.")]
    public bool enableTearMeshTriggerWhenGrabbed = false;

    [Tooltip("Start in normal mode on enable.")]
    public bool startInNormalMode = true;

    [Tooltip("If no normal colliders are assigned/found, keep mesh collider active in normal mode.")]
    public bool fallbackToMeshWhenNoNormalColliders = true;

    bool _subscribed;

    void Reset()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void Awake()
    {
        if (!grab) grab = GetComponent<XRGrabInteractable>();

        if (!tearMeshCollider)
        {
            var meshes = GetComponentsInChildren<MeshCollider>(true);
            for (int i = 0; i < meshes.Length; i++)
            {
                if (meshes[i] != null)
                {
                    tearMeshCollider = meshes[i];
                    break;
                }
            }
        }

        if (normalColliders == null || normalColliders.Length == 0)
        {
            var all = GetComponentsInChildren<Collider>(true);
            var list = new List<Collider>(all.Length);
            for (int i = 0; i < all.Length; i++)
            {
                var c = all[i];
                if (!c) continue;
                if (c == tearMeshCollider) continue;
                list.Add(c);
            }
            normalColliders = list.ToArray();
        }
    }

    void OnEnable()
    {
        SubscribeGrabEvents();
        if (startInNormalMode) ApplyNormalMode();
    }

    void OnDisable()
    {
        UnsubscribeGrabEvents();
    }

    void SubscribeGrabEvents()
    {
        if (_subscribed || !grab) return;
        grab.selectEntered.AddListener(OnGrabEnter);
        grab.selectExited.AddListener(OnGrabExit);
        _subscribed = true;
    }

    void UnsubscribeGrabEvents()
    {
        if (!_subscribed || !grab) return;
        grab.selectEntered.RemoveListener(OnGrabEnter);
        grab.selectExited.RemoveListener(OnGrabExit);
        _subscribed = false;
    }

    void OnGrabEnter(SelectEnterEventArgs _)
    {
        ApplyGrabMode();
    }

    void OnGrabExit(SelectExitEventArgs _)
    {
        ApplyNormalMode();
    }

    public void ApplyNormalMode()
    {
        bool hasNormal = HasNormalColliders();
        SetNormalCollidersEnabled(true);

        if (tearMeshCollider)
        {
            tearMeshCollider.enabled = !hasNormal && fallbackToMeshWhenNoNormalColliders;
            tearMeshCollider.isTrigger = false;
        }
    }

    public void ApplyGrabMode()
    {
        bool hasNormal = HasNormalColliders();
        SetNormalCollidersEnabled(keepNormalCollidersWhenGrabbed);

        if (tearMeshCollider)
        {
            if (enableTearMeshTriggerWhenGrabbed)
            {
                tearMeshCollider.enabled = true;
                tearMeshCollider.isTrigger = true;
            }
            else
            {
                tearMeshCollider.enabled = !hasNormal && fallbackToMeshWhenNoNormalColliders;
                tearMeshCollider.isTrigger = false;
            }
        }
    }

    public void ApplyTearMode()
    {
        SetNormalCollidersEnabled(false);

        if (!tearMeshCollider) return;

        // Tear phase uses convex mesh for robust dynamic interactions.
        tearMeshCollider.convex = true;
        tearMeshCollider.isTrigger = false;
        tearMeshCollider.enabled = true;
    }

    void SetNormalCollidersEnabled(bool enabled)
    {
        if (normalColliders == null) return;
        for (int i = 0; i < normalColliders.Length; i++)
        {
            if (normalColliders[i])
                normalColliders[i].enabled = enabled;
        }
    }

    bool HasNormalColliders()
    {
        if (normalColliders == null) return false;
        for (int i = 0; i < normalColliders.Length; i++)
        {
            if (normalColliders[i]) return true;
        }
        return false;
    }
}
