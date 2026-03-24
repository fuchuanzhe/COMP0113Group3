using System.Collections.Generic;
using UnityEngine;

public class KnifeCutter : MonoBehaviour
{
    public Collider bladeVolume;
    public Transform bladeTip;
    public Transform bladeBase;

    public LayerMask cuttableLayers = ~0;
    public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

    public float minBladeSpeed = 0.6f;          
    public float minCutTravel = 0.08f;        
    public int minPenetrationFrames = 3;        

    public float throwSpeed = 1.0f;
    public bool useKnifeForwardForThrow = true;

    public AudioClip cutSound;             
    [Range(0f, 1f)]
    public float soundVolume = 1.0f;        

    Rigidbody _rb;

    Vector3 _lastBladeWorldPos;
    bool _hasLastBladePos;

    class ContactState
    {
        public Vector3 enterPoint;
        public Vector3 lastPoint;
        public float travel;
        public int framesInside;
    }

    readonly Dictionary<CuttableObject, ContactState> _states = new();
    readonly Collider[] _overlaps = new Collider[64];

    void Awake()
    {
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

        var b = bladeVolume.bounds;
        var center = b.center;
        var halfExtents = b.extents;
        var rot = bladeVolume.transform.rotation;

        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlaps, rot, cuttableLayers, queryTriggerInteraction);
        var touchedThisFrame = new List<CuttableObject>(Mathf.Min(count, 16));

        for (int i = 0; i < count; i++)
        {
            var col = _overlaps[i];
            if (!col) continue;

            var cuttable = col.GetComponentInParent<CuttableObject>();
            if (!cuttable || cuttable.IsCut) continue;

            Vector3 p = col.ClosestPoint(center);

            if (!_states.TryGetValue(cuttable, out var st))
            {
                st = new ContactState
                {
                    enterPoint = p,
                    lastPoint = p,
                    travel = 0f,
                    framesInside = 0
                };
                _states[cuttable] = st;
            }
            st.framesInside++;
            st.travel += Vector3.Distance(st.lastPoint, p);
            st.lastPoint = p;

            touchedThisFrame.Add(cuttable);
        }
        if (_states.Count > 0)
        {
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
                    OnBladeExit(cuttable, center);
                    _states.Remove(cuttable);
                }
            }

            ListPool<CuttableObject>.Release(keys);
        }

        // Update fallback blade position
        _lastBladeWorldPos = center;
        _hasLastBladePos = true;
    }

    float KnifeSpeed(Vector3 bladeCenter)
    {
        if (_rb) return _rb.linearVelocity.magnitude;
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
        Vector3 pathDir = (exitPoint - st.enterPoint);
        Vector3 bladeDir = BladeDirection();
        Vector3 planeNormal = Vector3.Cross(bladeDir, pathDir).normalized;
        if (planeNormal.sqrMagnitude < 1e-4f)
            planeNormal = transform.right;

        Vector3 throwDir = useKnifeForwardForThrow ? bladeDir : (pathDir.sqrMagnitude > 1e-6f ? pathDir.normalized : bladeDir);

        if (cutSound != null)
        {
            AudioSource.PlayClipAtPoint(cutSound, cutCenter, soundVolume);
        }

        cuttable.DoCut(cutCenter, planeNormal, throwDir * throwSpeed);
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
            if (_pool.Count < 32)
                _pool.Push(list);
        }
    }

}
