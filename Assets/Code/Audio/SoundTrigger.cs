using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AudioSystem

{
    public enum PlayType
    {
        OnClick,
        OnEnable
    }

    public class SoundTrigger : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("What sound should be played? If multiple sound names are provded, a random sound will be played.")]
        private string[] soundName;

        [SerializeField]
        [Tooltip("What event should trigger the sound? \n - Use OnClick for buttons or toggles.\n - Use OnEnable for the sound to play as soon as the object is enabled.")]
        private PlayType playType;
        private List<SoundData> _soundDataList = new List<SoundData>();

        private void _applyListener()
        {
            // Add a listener to the button if there is one
            gameObject.TryGetComponent<Button>(out var button);
            if (button != null)
            {
                button.onClick.AddListener(_playSound);
            }

            // Add a listener to the toggle if there is one
            gameObject.TryGetComponent<Toggle>(out var toggle);
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(_toggleChanged);
            }
        }

        private void Start()
        {
            // Populate the sound data list
            if (soundName.Length == 0)
            {
                Debug.LogError("No sound name has been set for the SoundTrigger component on " + gameObject.name);
                return;
            }

            if (soundName.Length > 1)
            {
                foreach (string sound in soundName)
                {
                    _soundDataList.Add(AudioManager.Instance.SoundList.GetSound(sound));
                }
            }
            else
            {
                _soundDataList.Add(AudioManager.Instance.SoundList.GetSound(soundName[0]));
            }

            // Add the button/toggle listener
            if (playType == PlayType.OnClick)
            {
                _applyListener();
            }
        }

        private void OnEnable()
        {
            if (playType == PlayType.OnEnable)
            {
                _playSound();
            }
        }

        private void _toggleChanged(bool isOn)
        {
            _playSound();
        }

        private void _playSound()
        {
            AudioManager.Instance.CreateSound()
            .Play(_soundDataList);
        }
    }
}