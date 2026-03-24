using System.Collections;
using System.Linq;
using UnityEngine;
using TMPro;
using Ubiq.Rooms;
using Ubiq.Messaging;

public class AssignSeats : MonoBehaviour
{
    public int requiredPlayers = 2;
    public Transform[] seats = new Transform[4];
    public Transform avatarRoot;
    private RoomClient roomClient;
    private bool started;
    public PlayerOccupation occupation;
    public GameObject occupationUI;
    private Coroutine uiCoroutine;
    public TMP_Text occupationText;
    public BgmPlaylistManager bgmManager;

    private NetworkContext context;
    private struct Message
    {
        public bool start;

        public Message(bool start)
        {
            this.start = start;
        }
    }


    private void Start()
    {
        context = NetworkScene.Register(this);
        roomClient = RoomClient.Find(this);

        roomClient.OnJoinedRoom.AddListener(_ =>
        {
            started = false;
        });

        roomClient.OnPeerRemoved.AddListener(_ => CleanupSeats());
        occupation = GetComponent<PlayerOccupation>();
        occupationUI.SetActive(false);

        if (!bgmManager)
            bgmManager = FindAnyObjectByType<BgmPlaylistManager>();
    }

    // triggered by button in waiting room
    public void GameStart()
    {
        if (!started)
        {
            started = true;
            var i = MoveToSeats();
            ShowOccupationUIFor3Seconds(i);
            bgmManager?.PlayMainGameSong();
            context.SendJson(new Message(true));
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var m = message.FromJson<Message>();
        if (m.start)
        {
            started = m.start;
            var i = MoveToSeats();
            ShowOccupationUIFor3Seconds(i);
            bgmManager?.PlayMainGameSong();
        }
    }

    private int MoveToSeats()
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
        return index;
    }

    private void ShowOccupationUIFor3Seconds(int i)
    {
        if (occupationUI == null)
            return;

        if (uiCoroutine != null)
        {
            StopCoroutine(uiCoroutine);
        }
        UpdateOccupationText();
        uiCoroutine = StartCoroutine(ShowOccupationUICoroutine(i));
    }

    // display ui with occupation info when game begins
    private IEnumerator ShowOccupationUICoroutine(int i)
    {
        if(i >= 2)
        {
            occupationUI.transform.rotation = Quaternion.Euler(0,180f,0);
        }
        occupationUI.SetActive(true);
        yield return new WaitForSeconds(3f);
        occupationUI.SetActive(false);
        uiCoroutine = null;
    }
    private void UpdateOccupationText()
    {
        string job = occupation.MyOccupation.ToString();
        string allowed = "";
        string blocked = "";

        if (occupation.MyOccupation == PlayerOccupation.Occupation.Chef)
        {
            allowed = "knife";
            blocked = "hammer";
        }
        else if (occupation.MyOccupation == PlayerOccupation.Occupation.Butcher)
        {
            allowed = "hammer";
            blocked = "knife";
        }

        occupationText.text = $"You are a {job}.\nYou can only grab {allowed} but not {blocked}!";
    }

    private void CleanupSeats()
    {
        var activeUuids = roomClient.Peers.Select(p => p.uuid).Append(roomClient.Me.uuid).OrderBy(x => x).ToList();

        // remove room property if player exited
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
