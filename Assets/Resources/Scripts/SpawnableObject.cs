using UnityEngine;
using Ubiq.Messaging;

public class SpawnableObject : MonoBehaviour
{
    public NetworkId NetworkId { get; set; }
    private NetworkContext context;
    private Vector3 lastPosition;

    private struct Message
    {
        public Vector3 position;
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
            context.SendJson(new Message()
            {
                position = transform.localPosition
            });
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        transform.localPosition = m.position;
        lastPosition = transform.localPosition;
    }
}
