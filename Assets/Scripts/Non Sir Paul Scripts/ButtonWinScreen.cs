using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonWinScreen : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void LoadNextLevel()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is not set in ButtonWinScreen!");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        // This will close the application when it's built and running
        Application.Quit();
        Debug.LogWarning("Quitting the game...");

        // This allows you to verify it works while testing inside the Unity Editor
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}