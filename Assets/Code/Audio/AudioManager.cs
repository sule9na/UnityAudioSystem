using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using System.Collections;

namespace AudioSystem
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        //MUSIC PROPERTIES
        [SerializeField]
        private AudioSource _musicSource1;

        [SerializeField]
        private AudioSource _musicSource2;

        private float _musicMaxVolume = 5f;

        [SerializeField]
        public MusicList musicList;

        [SerializeField]
        private AudioMixer _musicMixer;

        private float _currentTrackFadeOutTime = 0f;

        private float _nextTrackFadeInTime = 0f;

        private AudioSource _lastMusicSourceUsed = null;

        //SFX PROPERTIES
        IObjectPool<SoundEmitter> soundEmitterPool;
        readonly List<SoundEmitter> activeSoundEmitters = new();
        public readonly LinkedList<SoundEmitter> FrequentSoundEmitters = new();
        public SoundList SoundList { get { return soundList; } }
        [SerializeField]
        public SoundList soundList;
        public float currentSFXVolume;
        [SerializeField]
        private AudioMixer _sfxMixer;
        [SerializeField]
        SoundEmitter soundEmitterPrefab;
        [SerializeField]
        bool collectionCheck = true;
        [SerializeField]
        [Tooltip("The initial number of sound emitters to create in the pool.")]
        int defaultCapacity = 10;
        [SerializeField]
        [Tooltip("The maximum number of sound emitters that will be created in the pool, based on demand.")]
        int maxPoolSize = 100;
        [SerializeField]
        [Tooltip("The maximum number of instances of any individual sound that can be played at once.")]
        int maxSoundInstances = 30;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }
        void Start()
        {
            //Setup the music source and mixer group
            _musicSource1.outputAudioMixerGroup = _musicMixer.FindMatchingGroups("Master")[0];
            _musicSource2.outputAudioMixerGroup = _musicMixer.FindMatchingGroups("Master")[0];

            //Load the current sound level from your player data service here e.g.
            //currentSFXVolume = PlayerProfile.Instance.GetSoundLevel();

            // TODO: Load the music level from your player profile service here e.g.
            // TODO: Remove the below line when the ProfileManager is implemented
            var musicLevel = 1f;

            if (musicLevel == 0)
            {
                _musicSource1.Pause();
                _musicSource2.Pause();
            }


            //Initialize the sound effects emitter pool
            InitializePool();
        }


        // SOUND EFFECT METHODS BELOW
        public SoundBuilder CreateSound() => new SoundBuilder(this);

        //Get sound emitter from pool public method
        public SoundEmitter Get()
        {
            return soundEmitterPool.Get();
        }

        public void ReturnToPool(SoundEmitter soundEmitter)
        {
            soundEmitterPool.Release(soundEmitter);
        }

        public bool CanPlaySound(SoundData data)
        {
            //If sound effects are muted just return false immediately
            if (currentSFXVolume == 0) return false;

            //If the clip is null then don't play the sound and log
            if (data.clip == null)
            {
                Debug.Log("Tried to play sound but no clip is set");
                return false;
            }

            //If it's not marked as a frequent sound assume we can play it
            if (!data.frequentSound) return true;

            //If it is, stop the oldest frequent sound and return that emmitter to the pool
            if (FrequentSoundEmitters.Count >= maxSoundInstances)
            {
                try
                {
                    FrequentSoundEmitters.First.Value.Stop();
                    return true;
                }
                catch
                {
                    Debug.Log("SoundEmitter is already released");
                }
                return false;
            }
            return true;
        }

        //When taking an emitter from the pool set it as active and add to list
        void OnTakeFromPool(SoundEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(true);
            activeSoundEmitters.Add(soundEmitter);
        }

        //When removing it from the pool do the set it as inactive and remove it from the list
        void OnReturnedToPool(SoundEmitter soundEmitter)
        {
            if (soundEmitter.Node != null)
            {
                FrequentSoundEmitters.Remove(soundEmitter.Node);
                soundEmitter.Node = null;
            }
            soundEmitter.gameObject.SetActive(false);
            activeSoundEmitters.Remove(soundEmitter);
        }

        void OnDestroyPoolObject(SoundEmitter soundEmitter)
        {
            Destroy(soundEmitter.gameObject);
        }

        //Create an emitter and set it as inactive, then return it
        SoundEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        void InitializePool()
        {
            soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaultCapacity,
                maxPoolSize
            );
        }

        public void StopSoundByName(string name)
        {
            // Collect emitters to stop in a new list, so the original isn't modified during enumeration
            List<SoundEmitter> emittersToStop = new List<SoundEmitter>();
            foreach (var soundEmitter in activeSoundEmitters)
            {
                if (soundEmitter.audioSource.name == name)
                {
                    emittersToStop.Add(soundEmitter);
                }
            }

            // Stop and remove the collected emitters
            foreach (var emitter in emittersToStop)
            {
                emitter.Stop();
            }
        }

        public void StopAllSounds()
        {
            List<SoundEmitter> emittersToStop = new List<SoundEmitter>();
            foreach (var soundEmitter in activeSoundEmitters)
            {
                emittersToStop.Add(soundEmitter);
            }
        }


        public void SetSoundVolume(float volume)
        {
            // Clamp the volume value to avoid Log10 of 0
            var volumeClamped = Mathf.Clamp(volume, 0.0001f, 1f);

            // Apply logarithmic scaling with base 10
            var dB = Mathf.Log10(volumeClamped) * 20;
            _sfxMixer.SetFloat("SoundVolume", dB);

            Debug.Log("Sound volume set to: " + dB);

            currentSFXVolume = volume;

            if (volume == 0)
            {
                StopAllSounds();
            }
        }

        // MUSIC METHODS BELOW
        public void PlayTrack(MusicData data)
        {
            PlayTrack(data, 0f);
        }
        public void PlayTrack(MusicData data, float crossFadeTime)
        {
            // Null check and warning in case references are missing
            if (data == null)
            {
                Debug.LogWarning("No music data found, check that music list is specified on the Music Manager object in your scene.");
                return;
            }

            // Determine which source to use
            AudioSource nextMusicSource = null;
            AudioSource currentMusicSource = null;
            if (_lastMusicSourceUsed == null)
            {
                _lastMusicSourceUsed = _musicSource1;
            }

            if (_lastMusicSourceUsed == _musicSource1)
            {
                nextMusicSource = _musicSource2;
                currentMusicSource = _musicSource1;
            }
            else
            {
                nextMusicSource = _musicSource1;
                currentMusicSource = _musicSource2;
            }

            // Get all the settings from the music data passed in
            if (data.clip == null)
            {
                Debug.LogWarning("No clip data found, check that clip is correctly specified in your MusicList");
                return;
            }

            nextMusicSource.clip = data.clip;
            _musicMaxVolume = data.volume;
            nextMusicSource.loop = data.loop;
            _nextTrackFadeInTime = data.fadeInTimeSeconds;

            if (crossFadeTime > 0)
            {
                // Start fading in the new track
                FadeIn(nextMusicSource, () =>
                {
                    // After the fade-in is complete, update the last music source used
                    _lastMusicSourceUsed = nextMusicSource;
                });

                // Start fading out the old track after the crossfade time has elapsed
                StartCoroutine(DelayedFadeOut(currentMusicSource, crossFadeTime));
            }
            else
            {
                // Start fading out the old track
                FadeOut(currentMusicSource, () =>
                {
                    // After the fade-out is complete, start fading in the new track
                    // And store the fade-out time for the new track
                    _currentTrackFadeOutTime = data.fadeOutTimeSeconds;
                    FadeIn(nextMusicSource, () =>
                    {
                        // After the fade-in is complete, update the last music source used
                        _lastMusicSourceUsed = nextMusicSource;
                    });
                });
            }
        }

        private IEnumerator DelayedFadeOut(AudioSource musicSource, float delay)
        {
            yield return new WaitForSeconds(delay);
            FadeOut(musicSource);
        }

        public void FadeIn(AudioSource musicSource, Action onComplete = null)
        {
            StartCoroutine(FadingIn(musicSource, onComplete));
        }

        public void FadeOut(AudioSource musicSource, Action onComplete = null)
        {
            StartCoroutine(FadingOut(musicSource, onComplete));
        }

        private IEnumerator FadingIn(AudioSource musicSource, Action onComplete = null)
        {
            float targetVolume = _musicMaxVolume;
            float timeElapsed = 0f;

            // Ensure the audio starts with volume at 0
            musicSource.volume = 0f;

            // Start playing the new track
            musicSource.Play();

            while (timeElapsed < _nextTrackFadeInTime)
            {
                // Update elapsed time
                timeElapsed += Time.deltaTime;

                // Calculate the new volume using Mathf.Lerp
                musicSource.volume = Mathf.Lerp(0f, targetVolume, timeElapsed / _nextTrackFadeInTime);

                // Wait for the next frame
                yield return null;
            }

            // Ensure the volume is set to the target volume at the end of the fade
            musicSource.volume = targetVolume;

            // Callback on complete
            onComplete?.Invoke();
        }

        private IEnumerator FadingOut(AudioSource musicSource, Action onComplete = null)
        {
            float startVolume = musicSource.volume;
            float timeElapsed = 0f;

            while (timeElapsed < _currentTrackFadeOutTime)
            {
                timeElapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / _currentTrackFadeOutTime);
                yield return null;
            }

            // Ensure volume is set to 0 at the end of the fade
            musicSource.volume = 0f;
            musicSource.Stop();

            // Callback on complete
            onComplete?.Invoke();
        }

        public void PauseMusic()
        {
            _musicSource1.Pause();
            _musicSource2.Pause();
        }

        // Sets the volume; pass in a value from 0 to 1, it will be converted to decibels
        public void SetMusicVolume(float volume)
        {
            if (volume == 0)
            {
                PauseMusic();
            }
            else
            {
                //Unpause the last music source used
                if (_lastMusicSourceUsed == _musicSource1)
                {
                    _musicSource1.UnPause();
                }
                else
                {
                    _musicSource2.UnPause();
                }
            }

            // Clamp the volume value to avoid Log10 of 0
            var volumeClamped = Mathf.Clamp(volume, 0.0001f, 1f);

            // Apply logarithmic scaling with base 10
            var dB = Mathf.Log10(volumeClamped) * 20;
            _musicMixer.SetFloat("MusicVolume", dB);

            Debug.Log("Music volume set to: " + dB);
        }
    }
}