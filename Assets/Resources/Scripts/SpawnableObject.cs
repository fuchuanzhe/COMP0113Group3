using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;

public class SpawnableObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;
    private Vector3 lastPosition;

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

    void Start()
    {
        context = NetworkScene.Register(this);
    }

    void Update()
    {
        if(lastPosition != transform.position)
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

    public void BroadcastPosAndRot()
    {
        context.SendJson(new Message(transform, true));
    }
}
