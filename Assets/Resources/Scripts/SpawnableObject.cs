using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;

public class SpawnableObject : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public NetworkContext context;
    private Vector3 lastPosition;

    private bool GetInvalidState()
    {
        var tile = GetComponent<LetterTile>();
        return tile != null && tile.IsInvalid;
    }

    private struct Message
    {
        public bool isActive;
        public bool isInvalid;
        public Vector3 position;
        public Quaternion rotation;

        public Message(Transform transform, bool isActive, bool isInvalid)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isActive = isActive;
            this.isInvalid = isInvalid;
        }
    }

    void Start()
    {
        context = NetworkScene.Register(this);
    }

    void Update()
    {
        if (lastPosition != transform.position)
        {
            lastPosition = transform.position;
            context.SendJson(new Message(transform, gameObject.activeSelf, GetInvalidState()));
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        gameObject.SetActive(m.isActive);

        transform.position = m.position;
        lastPosition = transform.position;
        transform.rotation = m.rotation;

        var tile = GetComponent<LetterTile>();
        if (tile != null)
        {
            if (m.isInvalid)
                tile.SetInvalidRed();
            else
                tile.RestoreOriginalColor();
        }
    }

    public void BroadcastActiveSelf(bool isActive)
    {
        context.SendJson(new Message(transform, isActive, GetInvalidState()));
    }

    public void BroadcastPosAndRot()
    {
        context.SendJson(new Message(transform, true, GetInvalidState()));
    }
}
