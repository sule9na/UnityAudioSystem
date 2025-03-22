using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    [Serializable]

    public class SoundData
    {
        public string name;
        public AudioClip clip;

        public AudioMixerGroup mixerGroup;

        [Tooltip("This controls the volume of the sound relative to other sounds. The mixer group controls the level of all sounds.")]
        [Range(0, 1)]
        public float volume = 1;

        [Tooltip("Pitch will be randomly set when each instance of the sound is created. Leave both settings at 1 for no variation.")]
        public bool randomPitch;
        [Range(0, 2)]
        public float pitchMin = 1;

        [Range(0, 2)]
        public float pitchMax = 1;

        public bool playOnAwake;

        public bool frequentSound;

        [Header("Looping Settings")]
        [Tooltip("Note that looping sounds will never stop. You must start them WithName in your create call and stop them with StopSoundByName")]

        public bool loop;
        [Tooltip("You can have sounds fade even if they're not looping sounds, most one shot shouts are too short to warrant it though.")]
        public bool fadeOut;
        [Tooltip("The time defaults to 0, meaning there will be no fade.")]
        public float fadeOutTimeSeconds = 0;
        [Tooltip("You can have sounds fade even if they're not looping sounds, most one shot shouts are too short to warrant it though.")]
        public bool fadeIn;
        [Tooltip("The time defaults to 0, meaning there will be no fade.")]
        public float fadeInTimeSeconds = 0;
    }

}