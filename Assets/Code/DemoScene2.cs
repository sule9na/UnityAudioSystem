using AudioSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DemoScene2 : MonoBehaviour
{
    [SerializeField]
    private Button _nextSceneButton;

    [SerializeField]
    private string _nextSceneName;

    void Start()
    {
        _nextSceneButton.onClick.AddListener(LoadNextScene);
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
