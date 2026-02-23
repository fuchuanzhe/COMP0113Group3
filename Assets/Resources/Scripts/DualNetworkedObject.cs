using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DualNetworkedObject : MonoBehaviour
{
    private NetworkContext context;
    private RoomClient roomClient;
    private Vector3 lastPosition;
    private string id;
    private XRGrabInteractable grab;
    private Rigidbody rb;

    private struct Message
    {
        public bool isActive;
        public Vector3 position;
        public Quaternion rotation;

        public Message(Transform transform, bool isActive)
        {
            this.isActive = isActive;
            this.position = transform.position;
            this.rotation = transform.rotation;
        }
    }

    private string MyKey => id + "_" + roomClient.Me.uuid;
    private int localGrabCount = 0;

    void Start()
    {
        context = NetworkScene.Register(this);
        roomClient = FindAnyObjectByType<RoomClient>();
        id = NetworkId.Create(this).ToString();
        grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }
        rb = GetComponent<Rigidbody>();
        roomClient.Room[MyKey] = "0";
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        localGrabCount++;
        roomClient.Room[MyKey] = localGrabCount.ToString();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        localGrabCount = Mathf.Max(0, localGrabCount - 1);
        roomClient.Room[MyKey] = localGrabCount.ToString();
    }

    private int GetTotalGrabCount()
    {
        int total = 0;
        foreach (var peer in roomClient.Peers)
        {
            string key = id + "_" + peer.uuid;
            string val = roomClient.Room[key];
            if (int.TryParse(val, out int count))
                total += count;
        }
        total += localGrabCount;
        return total;
    }

    void Update()
    {
        int total = GetTotalGrabCount();

        if (total >= 2)
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            return;
        }

        rb.constraints = RigidbodyConstraints.None;
        if (lastPosition != transform.localPosition)
        {
            lastPosition = transform.localPosition;
            context.SendJson(new Message(transform, gameObject.activeSelf));
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        gameObject.SetActive(m.isActive);
        
        transform.localPosition = m.position;
        lastPosition = transform.localPosition;
        transform.rotation = m.rotation;
    }

    public void BroadcastActiveSelf(bool isActive)
    {
        context.SendJson(new Message(transform, isActive));
    }
}