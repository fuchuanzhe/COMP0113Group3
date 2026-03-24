using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;

// used for objects that are created during runtime
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

    private bool GetSceneObjectWordState()
    {
        var tile = GetComponent<LetterTile>();
        return tile != null && tile.IsSceneObjectWord;
    }

    private struct Message
    {
        public bool isActive;
        public bool isInvalid;
        public bool isSceneObjectWord;
        public Vector3 position;
        public Quaternion rotation;

        public Message(Transform transform, bool isActive, bool isInvalid, bool isSceneObjectWord)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.isActive = isActive;
            this.isInvalid = isInvalid;
            this.isSceneObjectWord = isSceneObjectWord;
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
            context.SendJson(new Message(
                transform,
                gameObject.activeSelf,
                GetInvalidState(),
                GetSceneObjectWordState()
            ));
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
        // set appearance of letters depending on state
        if (tile != null)
        {
            if (m.isSceneObjectWord)
                tile.SetSceneObjectYellow();
            else if (m.isInvalid)
                tile.SetInvalidRed();
            else
                tile.RestoreOriginalColor();
        }
    }

    public void BroadcastActiveSelf(bool isActive)
    {
        context.SendJson(new Message(
            transform,
            isActive,
            GetInvalidState(),
            GetSceneObjectWordState()
        ));
    }

    public void BroadcastPosAndRot()
    {
        context.SendJson(new Message(
            transform,
            true,
            GetInvalidState(),
            GetSceneObjectWordState()
        ));
    }
}