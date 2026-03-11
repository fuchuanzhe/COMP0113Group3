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
    public PlayerOccupation occupation;

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
        roomClient.OnPeerRemoved.AddListener(_ => CleanupSeats());
        occupation = GetComponent<PlayerOccupation>();
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

        string key = $"seat_{index}";

        // TO BE HANDLED: may cause problem if players > seats 
        while (!string.IsNullOrEmpty(roomClient.Room[key]))
        {
            index = (index + 1) % seats.Length;
            key = $"seat_{index}";
        }

        occupation.SetFromSeatIndex(index);

        avatarRoot.position = seats[index].position;
        avatarRoot.rotation = seats[index].rotation;

        roomClient.Room[key] = roomClient.Me.uuid;  

        Debug.Log($"Assigned seat {index}.");
        Debug.Log($"Assigned occupation {occupation.MyOccupation}.");
    }

    private void CleanupSeats()
    {
        var activeUuids = roomClient.Peers.Select(p => p.uuid).Append(roomClient.Me.uuid).OrderBy(x => x).ToList();

        for (int i = 0; i < seats.Length; i++)
        {
            string key = $"seat_{i}";
            string occupant = roomClient.Room[key];

            if (!string.IsNullOrEmpty(occupant) && !activeUuids.Contains(occupant))
            {
                roomClient.Room[key] = "";
            }
        }
    }
}
