using UnityEngine;
using Ubiq.Messaging;

public class NetworkedObject : MonoBehaviour
{
    private NetworkContext context;
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
        if(lastPosition != transform.localPosition)
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
