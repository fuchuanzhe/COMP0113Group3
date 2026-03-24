using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Rooms;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class NetworkedObject : MonoBehaviour
{
    private NetworkContext context;
    private RoomClient roomClient;
    private Vector3 lastPosition;
    private string id;
    private XRGrabInteractable grab;
    private struct Message
    {
        public bool isActive;
        public Vector3 position;
        public Quaternion rotation;

        public Message(Transform transform, bool isActive)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isActive = isActive;
        }
    }

    private string LocalId => roomClient.Me.uuid;
    private string currentOwner;

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
        roomClient.OnRoomUpdated.AddListener(OnRoomUpdated);
    }

    private bool IsOwner()
    {
        return currentOwner == LocalId;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // release grab if object is already grabbed by someone else
        if (!string.IsNullOrEmpty(currentOwner) && currentOwner != LocalId)
        {
            var interactor = args.interactorObject;
            grab.interactionManager.SelectExit(interactor, grab);
            return;
        }
        roomClient.Room[id] = LocalId;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // set owner to no one once let go
        if (currentOwner == LocalId) roomClient.Room[id] = "";
    }

    private void OnRoomUpdated(IRoom room)
    {
        currentOwner = roomClient.Room[id];
    }

    void Update()
    {
        if(IsOwner() && lastPosition != transform.position)
        {
            lastPosition = transform.position;
            context.SendJson(new Message(transform, gameObject.activeSelf));
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        gameObject.SetActive(m.isActive);

        transform.position = m.position;
        lastPosition = transform.position;
        transform.rotation = m.rotation;
    }

    public void BroadcastActiveSelf(bool isActive)
    {
        context.SendJson(new Message(transform, isActive));
    }
}
