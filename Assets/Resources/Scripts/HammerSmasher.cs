using System.Collections.Generic;
using UnityEngine;

public class HammerSmasher : MonoBehaviour
{
    public Collider headVolume; 
    public LayerMask smashableLayers = ~0;
    public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

    public float minHeadSpeed = 1.2f;        
    public float minImpactCooldown = 0.15f;  

    public AudioClip smashSound;               
    [Range(0f, 1f)]
    public float soundVolume = 1.0f;         

    Rigidbody _rb;
    Vector3 _lastHeadPos;
    bool _hasLast;
    float _cooldownUntil;

    readonly Collider[] _overlaps = new Collider[64];
    readonly HashSet<SmashableObject> _prevTouching = new HashSet<SmashableObject>();

    void Awake()
    {
        _rb = GetComponentInParent<Rigidbody>();
        if (!_rb) _rb = GetComponent<Rigidbody>();

        if (headVolume)
        {
            _lastHeadPos = headVolume.bounds.center;
            _hasLast = true;
        }
    }

    void FixedUpdate()
    {
        if (!headVolume) return;
        if (Time.time < _cooldownUntil) { UpdateHeadPos(); return; }

        if (!IsHeld())
        {
            _prevTouching.Clear();
            UpdateHeadPos();
            return;
        }

        var b = headVolume.bounds;
        var center = b.center;
        var halfExtents = b.extents;
        var rot = headVolume.transform.rotation;

        // Detect smashable objects overlapping with the head volume
        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlaps, rot, smashableLayers, queryTriggerInteraction);
        var currTouching = ListPool<SmashableObject>.Get();

        for (int i = 0; i < count; i++)
        {
            var col = _overlaps[i];
            if (!col) continue;

            var smashable = col.GetComponentInParent<SmashableObject>();
            if (!smashable || smashable.IsSmashed) continue;

            currTouching.Add(smashable);

            if (_prevTouching.Contains(smashable))
                continue;

            float speed = HeadSpeed(center);
            if (speed < minHeadSpeed)
                continue;
            // Compute impact point
            Vector3 hitPoint = col.ClosestPoint(center);

            if (smashSound != null)
            {
                AudioSource.PlayClipAtPoint(smashSound, hitPoint, soundVolume);
            }

            smashable.DoSmash(hitPoint);

            _cooldownUntil = Time.time + minImpactCooldown;
            break;
        }

        _prevTouching.Clear();
        for (int i = 0; i < currTouching.Count; i++)
            _prevTouching.Add(currTouching[i]);

        ListPool<SmashableObject>.Release(currTouching);

        UpdateHeadPos();
    }

    void UpdateHeadPos()
    {
        if (!headVolume) return;
        _lastHeadPos = headVolume.bounds.center;
        _hasLast = true;
    }
    
    // Compute the speed of hammer head movement
    float HeadSpeed(Vector3 headCenter)
    {
        if (_rb) return _rb.linearVelocity.magnitude;
        if (_hasLast)
            return Vector3.Distance(_lastHeadPos, headCenter) / Time.fixedDeltaTime;

        return 0f;
    }

    Component _grab;
    System.Reflection.PropertyInfo _isSelectedProp;
    bool _grabCached;

    bool IsHeld()
    {
        if (!_grabCached)
        {
            _grabCached = true;

            var t = System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, Unity.XR.Interaction.Toolkit");

            if (t != null)
            {
                _grab = GetComponentInParent(t); 
                if (_grab != null)
                    _isSelectedProp = t.GetProperty("isSelected");
            }
        }

        if (_grab != null && _isSelectedProp != null)
        {
            var v = _isSelectedProp.GetValue(_grab);
            if (v is bool b) return b;
        }

        return false;
    }

    static class ListPool<T>
    {
        static readonly Stack<List<T>> _pool = new Stack<List<T>>(8);
        public static List<T> Get()
        {
            if (_pool.Count > 0)
            {
                var list = _pool.Pop();
                list.Clear();
                return list;
            }
            return new List<T>(16);
        }
        public static void Release(List<T> list)
        {
            if (list == null) return;
            list.Clear();
            if (_pool.Count < 32) _pool.Push(list);
        }
    }

}
