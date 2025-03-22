using UnityEngine;
using AudioSystem;

[CreateAssetMenu(menuName = "Audio/MusicList")]
public class MusicList : ScriptableObject
{
    [Header("MUSIC TRACKS")]
    [SerializeField]
    public MusicData[] musicTracks;

    public MusicData GetMusicTrack(string musicTrackName)
    {
        foreach (MusicData musicTrack in musicTracks)
        {
            if (musicTrack.name == musicTrackName)
            {
                Debug.Log("Found music track: " + musicTrackName);
                return musicTrack;
            }
        }
        return null;
    }
}