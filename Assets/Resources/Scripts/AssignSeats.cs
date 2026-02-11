using System.Linq;
using UnityEngine;
using Ubiq.Rooms;

public class AssignSeats : MonoBehaviour
{
    public int requiredPlayers = 2;
    public Transform[] seats = new Transform[4];
    public Transform avatarRoot;
    private RoomClient roomClient;
    private bool started;

    private void Start()
    {
        roomClient = RoomClient.Find(this);

        roomClient.OnJoinedRoom.AddListener(_ =>
        {
            started = false;
            Debug.Log("Joined");
            Check();
        });

        roomClient.OnPeerAdded.AddListener(_ => Check());
        roomClient.OnPeerRemoved.AddListener(_ => Check());
    }

    private void Check()
    {
        int remote = roomClient.Peers.Count();
        int total = remote + 1;

        Debug.Log($"Players: {total}/{requiredPlayers}");

        if (!started && total >= requiredPlayers)
        {
            started = true;
            MoveToSeats();
        }
    }

    private void MoveToSeats()
    {
        var allUuids = roomClient.Peers.Select(p => p.uuid).Append(roomClient.Me.uuid).OrderBy(x => x).ToList();
        int index = Mathf.Clamp(allUuids.IndexOf(roomClient.Me.uuid), 0, seats.Length - 1);

        avatarRoot.position = seats[index].position;
        avatarRoot.rotation = seats[index].rotation;

        Debug.Log($"Assigned seat {index}.");
    }
}
