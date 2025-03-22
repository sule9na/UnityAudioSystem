using System;
using UnityEngine;

namespace AudioSystem
{
    [Serializable]

    public class MusicData
    {
        [Tooltip("The name of the music track, that will be used to reference it in code")]
        public string name;

        [Tooltip("The audio clip file to play")]
        public AudioClip clip;

        [Tooltip("Should the music track loop?")]
        public bool loop;

        [Tooltip("The volume of the music track")]
        [Range(0, 1)]
        public float volume = 1;

        [Tooltip("The amount of time it takes for this track to fade in")]
        public float fadeInTimeSeconds = 0;

        [Tooltip("The amount of time it takes for this track to fade out")]
        public float fadeOutTimeSeconds = 0;
    }

}