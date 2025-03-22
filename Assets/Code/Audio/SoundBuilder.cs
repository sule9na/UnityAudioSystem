using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem
{
    public class SoundBuilder
    {
        readonly AudioManager soundManager;
        SoundData soundData;

        private string customName;

        Vector3 position = Vector3.zero;

        public SoundBuilder(AudioManager soundManager)
        {
            this.soundManager = soundManager;
        }

        public SoundBuilder WithName(string name)
        {
            this.customName = name;
            return this;
        }

        public SoundBuilder WithPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }

        public void Play(List<SoundData> soundDataList)
        {
            // Check if the list is not empty
            if (soundDataList == null || soundDataList.Count == 0)
            {
                Debug.LogError("Attempted to play a sound using an empty list of sound data.");
                return;
            }

            // Get a random sound from the list
            int randomIndex = UnityEngine.Random.Range(0, soundDataList.Count);
            SoundData selectedSoundData = soundDataList[randomIndex];

            // Delegate to the single SoundData version
            Play(selectedSoundData);
        }

        public void Play(SoundData soundData)
        {
            // Bail out early if sound data isn't present
            if (soundData == null)
            {
                Debug.LogError("No data is defined for this item.");
                return;
            }

            // Check if we can play the sound so we can bail out early if not
            if (!soundManager.CanPlaySound(soundData)) return;

            SoundEmitter soundEmitter = soundManager.Get();
            soundEmitter.Initialize(soundData, customName);
            soundEmitter.transform.position = position;
            soundEmitter.transform.parent = AudioManager.Instance.transform;

            // If it is a frequent sound, add it as the last item in the list
            if (soundData.frequentSound)
            {
                soundEmitter.Node = soundManager.FrequentSoundEmitters.AddLast(soundEmitter);
            }
            soundEmitter.Play();
        }

    }
}