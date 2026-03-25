using UnityEngine;
using System.Text.RegularExpressions;

public class TearObject : MonoBehaviour
{
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    public float tearDistance = 0.25f;
    public float armDelay = 0.08f;

    public bool isTeared { get; private set; }          
    public Vector3 tearCenter { get; private set; }     
    public string word { get; private set; }
    bool _armed;
    float _armedAt;
    public AudioClip clip;

    void Reset()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void Awake()
    {
        if (!grab) grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        // Extract uppercase letters from object name
        word = Regex.Replace(gameObject.name.ToUpperInvariant(), "[^A-Z]", "");
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

    // Tear the object apart
    void DoTear(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor a,
                UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor b,
                Vector3 posA,
                Vector3 posB)
    {
        isTeared = true;
        tearCenter = (posA + posB) * 0.5f;

        if (grab.interactionManager != null)
        {
            grab.interactionManager.SelectExit(a, grab);
            grab.interactionManager.SelectExit(b, grab);
        }

        LetterSpawner.Instance?.SpawnWord(word, tearCenter);
        if(clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
        var networkObj = GetComponent<NetworkedObject>();
        networkObj.BroadcastActiveSelf(false);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

}
