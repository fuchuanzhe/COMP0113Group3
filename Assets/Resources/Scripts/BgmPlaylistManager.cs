using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BgmPlaylistManager : MonoBehaviour
{
    public enum PlaylistType
    {
        None,
        WaitingRoom,
        MainGame
    }

    [Header("References")]
    public AudioSource source;

    [Header("Playlists")]
    public AudioClip[] waitingRoomPlaylist;
    public AudioClip[] mainGamePlaylist;

    [Header("Behavior")]
    public bool playWaitingRoomOnStart = true;
    public bool avoidImmediateRepeat = true;

    private PlaylistType currentPlaylist = PlaylistType.None;
    private Coroutine playRoutine;
    private int lastClipId = -1;

    public PlaylistType CurrentPlaylist => currentPlaylist;

    void Reset()
    {
        source = GetComponent<AudioSource>();
    }

    void Awake()
    {
        if (!source) source = GetComponent<AudioSource>();
        source.loop = false;
        source.spatialBlend = 0f;
        source.playOnAwake = false;
    }

    void Start()
    {
        if (playWaitingRoomOnStart)
            PlayWaitingRoomPlaylist();
    }

    void OnDisable()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }
    }

    public void PlayWaitingRoomPlaylist()
    {
        SwitchPlaylist(PlaylistType.WaitingRoom);
    }

    public void PlayMainGamePlaylist()
    {
        SwitchPlaylist(PlaylistType.MainGame);
    }

    void SwitchPlaylist(PlaylistType target)
    {
        if (currentPlaylist == target && playRoutine != null)
            return;

        currentPlaylist = target;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        source.Stop();
        playRoutine = StartCoroutine(PlayLoopRoutine(target));
    }

    IEnumerator PlayLoopRoutine(PlaylistType playlist)
    {
        while (currentPlaylist == playlist)
        {
            var clips = BuildValidClipList(playlist);
            if (clips.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            int pick = Random.Range(0, clips.Count);
            if (avoidImmediateRepeat && clips.Count > 1 && pick == lastClipId)
            {
                pick = (pick + Random.Range(1, clips.Count)) % clips.Count;
            }

            lastClipId = pick;
            source.clip = clips[pick];
            source.Play();

            while (currentPlaylist == playlist && source.isPlaying)
                yield return null;
        }
    }

    List<AudioClip> BuildValidClipList(PlaylistType playlist)
    {
        var src = playlist == PlaylistType.MainGame ? mainGamePlaylist : waitingRoomPlaylist;
        var list = new List<AudioClip>();
        if (src == null) return list;

        for (int i = 0; i < src.Length; i++)
        {
            if (src[i] != null)
                list.Add(src[i]);
        }

        return list;
    }
}
