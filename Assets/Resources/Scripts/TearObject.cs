using UnityEngine;

public class TearObject : MonoBehaviour
{
    [Header("XRI")]
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    [Header("Tear Condition")]
    public float tearDistance = 0.25f;
    public float armDelay = 0.08f;

    [Header("Spawn Halves (optional)")]
    public bool spawnHalves = true;
    public GameObject HalfPrefabA;
    public GameObject HalfPrefabB;
    public float spawnOffset = 0.02f;
    public float throwSpeed = 0.8f;
    public bool addRigidbodyIfMissing = true;

    [Header("State (read by LetterSpawner)")]
    public bool isTeared { get; private set; }          
    public Vector3 tearCenter { get; private set; }     
    public string word { get; private set; }            // Word to spawn

    bool _armed;
    float _armedAt;

    void Reset()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void Awake()
    {
        if (!grab) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        // Set word based on object name
        //word = CleanName(gameObject.name).ToUpperInvariant();
        word = gameObject.name.ToUpperInvariant();
    }

    void Update()
    {
        if (isTeared || grab == null) return;

        var list = grab.interactorsSelecting;
        if (list == null || list.Count < 2)
        {
            _armed = false;
            return;
        }

        if (!_armed)
        {
            _armed = true;
            _armedAt = Time.time;
        }
        if (Time.time - _armedAt < armDelay) return;

        var a = list[0];
        var b = list[1];

        Vector3 posA = GetInteractorPos(a);
        Vector3 posB = GetInteractorPos(b);

        if (Vector3.Distance(posA, posB) >= tearDistance)
        {
            DoTear(a, b, posA, posB);
        }
    }

    Vector3 GetInteractorPos(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor)
    {
        Transform t = interactor.GetAttachTransform(grab);
        return t ? t.position : interactor.transform.position;
    }

    void DoTear(
        UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor a,
        UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor b,
        Vector3 posA,
        Vector3 posB
    )
    {
        isTeared = true;
        tearCenter = (posA + posB) * 0.5f;

        if (grab.interactionManager != null)
        {
            grab.interactionManager.SelectExit(a, grab);
            grab.interactionManager.SelectExit(b, grab);
        }

        if (spawnHalves && HalfPrefabA && HalfPrefabB)
        {
            SpawnHalves(posA, posB);
        }

        // Deactivate original object after tearing.
        gameObject.SetActive(false);
    }

    void SpawnHalves(Vector3 posA, Vector3 posB)
    {
        Vector3 dir = (posB - posA);
        if (dir.sqrMagnitude < 1e-6f) dir = transform.right;
        dir.Normalize();

        Vector3 spawnPosA = posA - dir * spawnOffset;
        Vector3 spawnPosB = posB + dir * spawnOffset;

        Quaternion rot = transform.rotation;

        var halfA = Instantiate(HalfPrefabA, spawnPosA, rot);
        var halfB = Instantiate(HalfPrefabB, spawnPosB, rot);

        ApplyThrow(halfA, -dir * throwSpeed);
        ApplyThrow(halfB,  dir * throwSpeed);
    }

    void ApplyThrow(GameObject obj, Vector3 velocity)
    {
        if (throwSpeed <= 0f) return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (!rb && addRigidbodyIfMissing)
            rb = obj.AddComponent<Rigidbody>();

        if (rb)
            rb.linearVelocity = velocity;
    }

    // static string CleanName(string n)
    // {
    //     if (string.IsNullOrWhiteSpace(n)) return "";
    //     return n.Replace("(Clone)", "").Trim();
    // }
}