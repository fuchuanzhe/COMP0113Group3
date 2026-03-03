using System.Collections.Generic;
using UnityEngine;

public class HammerSmasher : MonoBehaviour
{
    [Header("Hammer Head Volume (no trigger needed)")]
    public Collider headVolume; // 锤头区域 collider（非trigger，建议子物体Box/Sphere）

    [Header("Physics Query")]
    public LayerMask smashableLayers = ~0;
    public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("Smash Condition")]
    public float minHeadSpeed = 1.2f;          // m/s：砸击最低速度
    public float minImpactCooldown = 0.15f;    // 秒：防止一次接触重复触发

    [Header("Optional: only smash while held")]
    public bool requireHeld = true;

    [Header("Impulse")]
    public float smashImpulse = 2.0f;          // 给碎片初速度的强度（更像“炸开”）
    public bool impulseAlongVelocity = true;

    Rigidbody _rb;
    Vector3 _lastHeadPos;
    bool _hasLast;
    float _cooldownUntil;

    readonly Collider[] _overlaps = new Collider[64];

    // 记录“上帧接触到的对象”，用于做 enter 检测（只在进入那一刻触发）
    readonly HashSet<SmashableObject> _prevTouching = new HashSet<SmashableObject>();

    void Awake()
    {
        if (!headVolume)
        {
            // 尽量找子物体 collider 当锤头
            var all = GetComponentsInChildren<Collider>(true);
            foreach (var c in all)
            {
                if (c && c != GetComponent<Collider>())
                {
                    headVolume = c;
                    break;
                }
            }
            if (!headVolume) headVolume = GetComponent<Collider>();
        }

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

        if (requireHeld && !IsHeld())
        {
            _prevTouching.Clear();
            UpdateHeadPos();
            return;
        }

        var b = headVolume.bounds;
        var center = b.center;
        var halfExtents = b.extents;
        var rot = headVolume.transform.rotation;

        int count = Physics.OverlapBoxNonAlloc(center, halfExtents, _overlaps, rot, smashableLayers, queryTriggerInteraction);

        // 当前帧接触集合
        var currTouching = ListPool<SmashableObject>.Get();

        for (int i = 0; i < count; i++)
        {
            var col = _overlaps[i];
            if (!col) continue;

            var smashable = col.GetComponentInParent<SmashableObject>();
            if (!smashable || smashable.IsSmashed) continue;

            currTouching.Add(smashable);

            // 只在“刚进入接触”的那一刻判断 smash（更像真正撞击）
            if (_prevTouching.Contains(smashable))
                continue;

            float speed = HeadSpeed(center);
            if (speed < minHeadSpeed)
                continue;

            Vector3 velDir = HeadVelocityDir(center);
            Vector3 impulse = impulseAlongVelocity ? velDir * smashImpulse : transform.up * smashImpulse;

            // smash point 用最近点
            Vector3 hitPoint = col.ClosestPoint(center);

            smashable.DoSmash(hitPoint, impulse);

            _cooldownUntil = Time.time + minImpactCooldown;
            break; // 一次挥击只砸一个，避免连锁
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

    float HeadSpeed(Vector3 headCenter)
    {
#if UNITY_2023_3_OR_NEWER
        if (_rb) return _rb.linearVelocity.magnitude;
#else
        if (_rb) return _rb.velocity.magnitude;
#endif
        if (_hasLast)
            return Vector3.Distance(_lastHeadPos, headCenter) / Time.fixedDeltaTime;

        return 0f;
    }

    Vector3 HeadVelocityDir(Vector3 headCenter)
    {
#if UNITY_2023_3_OR_NEWER
        if (_rb && _rb.linearVelocity.sqrMagnitude > 1e-6f) return _rb.linearVelocity.normalized;
#else
        if (_rb && _rb.velocity.sqrMagnitude > 1e-6f) return _rb.velocity.normalized;
#endif
        if (_hasLast)
        {
            var d = headCenter - _lastHeadPos;
            if (d.sqrMagnitude > 1e-6f) return d.normalized;
        }
        return transform.forward;
    }

// 放在 HammerSmasher 类里（字段区）
Component _grab;
System.Reflection.PropertyInfo _isSelectedProp;
bool _grabCached;

bool IsHeld()
{
    if (!requireHeld) return true;

    if (!_grabCached)
    {
        _grabCached = true;

        // 找到 XRGrabInteractable（不直接引用 XRI 类型，避免编译/版本/命名空间问题）
        // 常见类型名：UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
        var t =
            System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, Unity.XR.Interaction.Toolkit")
            ?? System.Type.GetType("UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable, Unity.XR.Interaction.Toolkit"); // 老版本兜底

        if (t != null)
        {
            _grab = GetComponentInParent(t); // 锤头子物体也能找到根上的 grab
            if (_grab != null)
                _isSelectedProp = t.GetProperty("isSelected");
        }
    }

    if (_grab != null && _isSelectedProp != null)
    {
        var v = _isSelectedProp.GetValue(_grab);
        if (v is bool b) return b;
    }

    // 找不到 grab 组件时：
    // 1) 如果你希望“必须抓住才砸”，这里 return false
    // 2) 如果你希望“即便没抓也能砸”（默认更宽松），这里 return true
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

    void OnDrawGizmosSelected()
    {
        if (!headVolume) return;
        var b = headVolume.bounds;
        Gizmos.matrix = Matrix4x4.TRS(b.center, headVolume.transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, b.size);
    }
}