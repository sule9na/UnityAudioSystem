using AudioSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemoScene : MonoBehaviour
{
    [SerializeField]
    private Button _playOneShotButton;

    [SerializeField]
    private Button _playLoopingSoundButton;

    [SerializeField]
    private Button _stopLoopingSoundButton;

    [SerializeField]
    private Button _switchMusicTrackButton;

    [SerializeField]
    private Button _crossfadeMusicButton;

    [SerializeField]
    private Slider _soundVolumeSlider;

    [SerializeField]
    private Slider _musicVolumeSlider;

    [SerializeField]
    private Button _nextSceneButton;

    [SerializeField]
    private string _nextSceneName;

    private int _currentMusicTrack = 0;

    void Start()
    {
        _playLoopingSoundButton.onClick.AddListener(PlayLoopingSound);
        _stopLoopingSoundButton.onClick.AddListener(StopLoopingSound);
        _switchMusicTrackButton.onClick.AddListener(SwitchMusicTrack);
        _crossfadeMusicButton.onClick.AddListener(CrossfadeMusic);
        _nextSceneButton.onClick.AddListener(LoadNextScene);
        _soundVolumeSlider.onValueChanged.AddListener(SetSoundVolume);
        _musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        AudioManager.Instance.PlayTrack(AudioManager.Instance.musicList.GetMusicTrack("Music1"));

        AudioManager.Instance.SetMusicVolume(0.5f);
        AudioManager.Instance.SetSoundVolume(0.5f);
    }

    private void PlayLoopingSound()
    {
        AudioManager.Instance.CreateSound()
        .WithName("Looping")
        .Play(AudioManager.Instance.SoundList.GetSound("Looping"));
    }

    private void StopLoopingSound()
    {
        AudioManager.Instance.StopSoundByName("Looping");
    }

    private void SwitchMusicTrack()
    {
        if (_currentMusicTrack == 2)
        {
            _currentMusicTrack = 1;
            AudioManager.Instance.PlayTrack(AudioManager.Instance.musicList.GetMusicTrack("Music1"));
        }
        else
        {
            _currentMusicTrack = 2;
            AudioManager.Instance.PlayTrack(AudioManager.Instance.musicList.GetMusicTrack("Music2"));
        }
    }

    private void CrossfadeMusic()
    {
        if (_currentMusicTrack == 2)
        {
            _currentMusicTrack = 1;
            AudioManager.Instance.PlayTrack(AudioManager.Instance.musicList.GetMusicTrack("Music1"), 5f);
        }
        else
        {
            _currentMusicTrack = 2;
            AudioManager.Instance.PlayTrack(AudioManager.Instance.musicList.GetMusicTrack("Music2"), 5f);
        }
    }

    public void SetMusicVolume(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
    }

    public void SetSoundVolume(float volume)
    {
        AudioManager.Instance.SetSoundVolume(volume);
    }

    private void LoadNextScene()
    {
        if (_nextSceneName != null)
        {
            SceneManager.LoadSceneAsync(_nextSceneName).completed += (AsyncOperation op) =>
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            };
        }
    }
}
