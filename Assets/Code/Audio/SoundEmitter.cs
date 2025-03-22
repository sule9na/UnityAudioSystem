using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;



namespace AudioSystem
{
    //[RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        public SoundData Data { get; private set; }
        public LinkedListNode<SoundEmitter> Node { get; set; }
        public AudioSource audioSource;

        public bool fadeOut;

        public float fadeOutTimeSeconds;

        public bool fadeIn;

        public float fadeInTimeSeconds;

        public float targetVolume;

        Coroutine playingCoroutine;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void Play()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
            }

            if (fadeIn)
            {
                StartCoroutine(FadeInCoroutine());
            }
            else
            {
                audioSource.Play();

                //Only start the waiting coroutine if the sound isn't looping. Looping sounds don't end
                if (!Data.loop)
                {
                    //Wait for the sound to end, then call the method to return the emitter to the pool
                    playingCoroutine = StartCoroutine(WaitForSoundToEnd());
                }
            }
        }

        public IEnumerator FadeInCoroutine()
        {
            float startVolume = 0f;
            float timeElapsed = 0f;

            audioSource.Play();

            while (timeElapsed < fadeInTimeSeconds)
            {
                timeElapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timeElapsed / fadeInTimeSeconds);
                yield return null;
            }

            // Ensure volume is set to targetvolume at the end of the fade
            audioSource.volume = targetVolume;
        }


        //Return emitter to pool when the sound has ended
        IEnumerator WaitForSoundToEnd()
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            Stop();
        }


        //Stop the emitter playing and then return the emitter to the pool
        public void Stop()
        {
            //Manually stop the playing coroutine if we manually stop the sound
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
                playingCoroutine = null;
            }
            //Tell the audiosource to stop playing
            if (fadeOut)
            {
                StartCoroutine(FadeOutCoroutine());
            }
            else
            {
                audioSource.Stop();
                AudioManager.Instance.ReturnToPool(this);
            }
        }

        public IEnumerator FadeOutCoroutine()
        {
            float startVolume = audioSource.volume;
            float timeElapsed = 0f;

            while (timeElapsed < fadeOutTimeSeconds)
            {
                timeElapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / fadeOutTimeSeconds);
                yield return null;
            }

            // Ensure volume is set to 0 at the end of the fade
            audioSource.volume = 0f;

            // Callback on complete
            audioSource.Stop();
            AudioManager.Instance.ReturnToPool(this);
        }

        public void Initialize(SoundData data, string customName = null)
        {
            Data = data;

            if (!string.IsNullOrEmpty(customName))
            {
                // Use the custom name if provided
                audioSource.name = customName;
            }
            else
            {
                //If there's no custom name set then use the clip name for easier debugging
                audioSource.name = data.clip.name;
            }

            if (data.randomPitch)
            {
                audioSource.pitch = Random.Range(data.pitchMin, data.pitchMax);
            }
            if (data.fadeIn)
            {
                audioSource.volume = 0f;
            }
            audioSource.volume = data.volume;
            audioSource.clip = data.clip;
            audioSource.outputAudioMixerGroup = data.mixerGroup;
            audioSource.loop = data.loop;
            audioSource.playOnAwake = data.playOnAwake;
            fadeOut = data.fadeOut;
            fadeOutTimeSeconds = data.fadeOutTimeSeconds;
            fadeIn = data.fadeIn;
            fadeInTimeSeconds = data.fadeInTimeSeconds;
            targetVolume = data.volume;
        }
    }

}