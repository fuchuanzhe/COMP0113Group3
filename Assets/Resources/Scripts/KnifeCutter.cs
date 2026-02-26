using System.Collections.Generic;
using UnityEngine;

public class KnifeCutter : MonoBehaviour
{
    [Header("Blade Volume (no trigger needed)")]
    [Tooltip("A collider that represents the blade volume used for cutting detection. It can be a child collider and does NOT need to be trigger.")]
    public Collider bladeVolume;

    [Tooltip("Optional: blade tip (for more stable blade direction)")]
    public Transform bladeTip;

    [Tooltip("Optional: blade base (for more stable blade direction)")]
    public Transform bladeBase;

    [Header("Physics Query")]
    [Tooltip("Only colliders on these layers will be considered cuttable.")]
    public LayerMask cuttableLayers = ~0;

    [Tooltip("Whether trigger colliders in the scene should be considered by the overlap query.")]
    public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Cut Condition")]
    public float minBladeSpeed = 0.6f;          // m/s
    public float minCutTravel = 0.08f;          // meters
    public int minPenetrationFrames = 3;        // frames inside

    [Header("Throw Halves")]
    public float throwSpeed = 1.0f;
    public bool useKnifeForwardForThrow = true;

    Rigidbody _rb;

    // Fallback speed tracking when rigidbody velocity is not reliable
    Vector3 _lastBladeWorldPos;
    bool _hasLastBladePos;

    class ContactState
    {
        public Vector3 enterPoint;
        public Vector3 lastPoint;
        public float travel;
        public int framesInside;
    }

    // Track state per cuttable object
    readonly Dictionary<CuttableObject, ContactState> _states = new();

    // Temp overlap list each frame
    readonly Collider[] _overlaps = new Collider[64];

    void Awake()
    {
        if (!bladeVolume)
        {
            // Try find a reasonable child collider (prefer child over root)
            var all = GetComponentsInChildren<Collider>(true);
            foreach (var c in all)
            {
                if (c != null && c != GetComponent<Collider>())
                {
                    bladeVolume = c;
                    break;
                }
            }

            // Fallback to self collider
            if (!bladeVolume)
                bladeVolume = GetComponent<Collider>();
        }

        // Rigidbody is usually on the grabbed knife root
        _rb = GetComponentInParent<Rigidbody>();
        if (!_rb) _rb = GetComponent<Rigidbody>();

        if (bladeVolume)
        {
            _lastBladeWorldPos = bladeVolume.bounds.center;
            _hasLastBladePos = true;
        }
    }

    void FixedUpdate()
    {
        if (!bladeVolume) return;

        // Overlap query based on the blade volume bounds
        // Note: bounds are world-axis aligned; extents are in world space.
        // Using bladeVolume.transform.rotation makes this closer to the blade orientation.
        var b = bladeVolume.bounds;
        var center = b.center;
        var halfExtents = b.extents;
        var rot = bladeVolume.transform.rotation;

        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlaps, rot, cuttableLayers, queryTriggerInteraction);

        // Build a set of cuttables currently intersecting the blade volume
        // (We avoid allocating a HashSet each frame by using a temp dictionary marker pattern.)
        // We'll mark touched states in this frame.
        var touchedThisFrame = new List<CuttableObject>(Mathf.Min(count, 16));

        for (int i = 0; i < count; i++)
        {
            var col = _overlaps[i];
            if (!col) continue;

            var cuttable = col.GetComponentInParent<CuttableObject>();
            if (!cuttable || cuttable.IsCut) continue;

            // Use closest point from blade center into the target collider to track travel
            Vector3 p = col.ClosestPoint(center);

            if (!_states.TryGetValue(cuttable, out var st))
            {
                // Enter
                st = new ContactState
                {
                    enterPoint = p,
                    lastPoint = p,
                    travel = 0f,
                    framesInside = 0
                };
                _states[cuttable] = st;
            }

            // Stay
            st.framesInside++;
            st.travel += Vector3.Distance(st.lastPoint, p);
            st.lastPoint = p;

            touchedThisFrame.Add(cuttable);
        }

        // Exit: any cuttable we were tracking but not touched this frame
        if (_states.Count > 0)
        {
            // Copy keys to avoid modifying dictionary while iterating
            var keys = ListPool<CuttableObject>.Get();
            keys.AddRange(_states.Keys);

            for (int k = 0; k < keys.Count; k++)
            {
                var cuttable = keys[k];
                if (cuttable == null)
                {
                    _states.Remove(cuttable);
                    continue;
                }

                if (!touchedThisFrame.Contains(cuttable))
                {
                    // Exit event
                    OnBladeExit(cuttable, center);
                    _states.Remove(cuttable);
                }
            }

            ListPool<CuttableObject>.Release(keys);
        }

        // Update fallback blade position for speed estimation
        _lastBladeWorldPos = center;
        _hasLastBladePos = true;
    }

    float KnifeSpeed(Vector3 bladeCenter)
    {
        // Prefer rigidbody velocity (works well with XRI)
#if UNITY_2023_3_OR_NEWER
        if (_rb) return _rb.linearVelocity.magnitude;
#else
        if (_rb) return _rb.velocity.magnitude;
#endif

        // Fallback: estimate from blade center movement
        if (_hasLastBladePos)
        {
            return Vector3.Distance(_lastBladeWorldPos, bladeCenter) / Time.fixedDeltaTime;
        }

        return 0f;
    }

    Vector3 BladeDirection()
    {
        if (bladeTip && bladeBase)
            return (bladeTip.position - bladeBase.position).normalized;

        return transform.forward;
    }

    void OnBladeExit(CuttableObject cuttable, Vector3 bladeCenter)
    {
        if (!cuttable || cuttable.IsCut) return;
        if (!_states.TryGetValue(cuttable, out var st)) return;

        float speed = KnifeSpeed(bladeCenter);
        bool speedOk = speed >= minBladeSpeed;
        bool travelOk = st.travel >= minCutTravel;
        bool framesOk = st.framesInside >= minPenetrationFrames;

        if (!(speedOk && travelOk && framesOk))
            return;

        // Use lastPoint as an approximation of exit point
        Vector3 exitPoint = st.lastPoint;
        Vector3 cutCenter = (st.enterPoint + exitPoint) * 0.5f;

        // Plane normal: cross(bladeDir, pathDir)
        Vector3 pathDir = (exitPoint - st.enterPoint);
        Vector3 bladeDir = BladeDirection();
        Vector3 planeNormal = Vector3.Cross(bladeDir, pathDir).normalized;
        if (planeNormal.sqrMagnitude < 1e-4f)
            planeNormal = transform.right;

        Vector3 throwDir = useKnifeForwardForThrow ? bladeDir : (pathDir.sqrMagnitude > 1e-6f ? pathDir.normalized : bladeDir);

        cuttable.DoCut(cutCenter, planeNormal, throwDir * throwSpeed);
    }

    // Tiny pooled list helper to avoid allocations when copying keys.
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
            if (_pool.Count < 32)
                _pool.Push(list);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!bladeVolume) return;
        var b = bladeVolume.bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, bladeVolume.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, b.size);
    }
}