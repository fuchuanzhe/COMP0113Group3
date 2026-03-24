using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BgmPlaylistManager : MonoBehaviour
{
    public enum SongType
    {
        None,
        WaitingRoom,
        MainGame
    }

    [Header("References")]
    public AudioSource source;

    [Header("Tracks")]
    public AudioClip waitingRoomClip;
    public AudioClip mainGameClip;

    [Header("Behavior")]
    public bool playWaitingRoomOnStart = true;

    private SongType currentSong = SongType.None;

    public SongType CurrentSong => currentSong;

    void Reset()
    {
        source = GetComponent<AudioSource>();
    }

    void Awake()
    {
        if (!source) source = GetComponent<AudioSource>();
        source.loop = true;
        source.spatialBlend = 0f;
        source.playOnAwake = false;
    }

    void Start()
    {
        if (playWaitingRoomOnStart)
            PlayWaitingRoomSong();
    }

    void OnDisable()
    {
        if (source != null)
            source.Stop();
    }

    public void PlayWaitingRoomSong()
    {
        PlaySong(SongType.WaitingRoom);
    }

    public void PlayMainGameSong()
    {
        PlaySong(SongType.MainGame);
    }

    void PlaySong(SongType target)
    {
        if (currentSong == target && source != null && source.isPlaying)
            return;

        currentSong = target;

        if (source == null)
            return;

        source.Stop();
        source.clip = target == SongType.MainGame ? mainGameClip : waitingRoomClip;

        if (source.clip != null)
            source.Play();
    }
}
