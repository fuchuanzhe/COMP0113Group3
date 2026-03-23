using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Rooms;

public class DualTearObject : MonoBehaviour
{
    private NetworkContext context;
    private XRGrabInteractable grab;
    public float tearDistance = 0.35f;
    private bool isGrabbed = false;
    private bool isPulled = false;
    private RoomClient roomClient;

    private bool otherGrabbed = false;
    private bool otherPulled = false;
    private string otherId;
    private Vector3? refControllerPos;
    public string word { get; private set; }

    // Audio configuration for the dual tear interaction
    [Header("Audio")]
    public AudioClip tearSound;
    [Range(0f, 1f)]
    public float soundVolume = 1.0f;

    private struct Message
    {
        public string id;
        public bool grabbed;
        public bool pulled;
        public Message(string id, bool grabbed, bool pulled)
        {
            this.id = id;
            this.grabbed = grabbed;
            this.pulled = pulled;
        }
    }

    void Start()
    {
        context = NetworkScene.Register(this);
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
        roomClient = RoomClient.Find(this);
        word = gameObject.name.ToUpperInvariant();
    }

    void Update()
    {
        if(!isGrabbed) return;
        var pos = GetControllerPos();
        context.SendJson(new Message(roomClient.Me.uuid, true, false));

        if(otherGrabbed && refControllerPos == null) refControllerPos = pos;

        if(refControllerPos != null)
        {
            float d = Vector3.Distance(pos, (Vector3)refControllerPos);
            isPulled = d >= tearDistance;
        }

        if (!isPulled) return;
        context.SendJson(new Message(roomClient.Me.uuid, true, true));

        if(otherPulled) DoTear();
    }

    private void OnGrab(SelectEnterEventArgs eventArgs)
    {
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        isPulled = false;
        refControllerPos = null;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        otherId = m.id;
        otherGrabbed = m.grabbed;
        otherPulled = m.pulled;
    }

    private Vector3 GetControllerPos()
    {
        var list = grab.interactorsSelecting;
        if (list != null && list.Count > 0)
        {
            Transform t = list[0].GetAttachTransform(grab);
            if (t != null) return t.position;
            return list[0].transform.position;
        }

        return Vector3.zero;
    }

    private Vector3 GetSpawnPosition()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return transform.position;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds.center;
    }

    private void DoTear()
    {
        // Play sound locally for both clients before the return check
        if (tearSound != null)
        {
            AudioSource.PlayClipAtPoint(tearSound, transform.position, soundVolume);
        }

        // only the client with smaller id spawns
        if(string.Compare(roomClient.Me.uuid, otherId) >= 0) return;

        Vector3 spawnPos = GetSpawnPosition();

        LetterSpawner.Instance?.SpawnWord(word, spawnPos);

        // LetterSpawner.Instance?.SpawnWord(word, transform.position);
        var networkObj = GetComponent<DualNetworkedObject>();
        networkObj.BroadcastActiveSelf(false);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}