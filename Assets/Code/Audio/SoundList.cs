using UnityEngine;
using AudioSystem;

[CreateAssetMenu(menuName = "Audio/SoundList")]
public class SoundList : ScriptableObject
{
    [Header("SOUND DATA")]
    [SerializeField]
    public SoundData[] soundData;

    public SoundData GetSound(string soundName)
    {
        foreach (SoundData sound in soundData)
        {
            if (sound.name == soundName)
            {
                return sound;
            }
        }
        return null;
    }
}


