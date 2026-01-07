using UnityEngine;
using UnityEngine.SceneManagement; // Required for switching scenes

public class SceneChanger : MonoBehaviour
{
    // A clean way to load by name (easier for menus)
    public void LoadScene(string sceneName)
    {
        // Check if the scene name is valid to prevent crashes
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' not found! Check your Build Settings.");
        }
    }

    // A clean way to load by Index (often safer for ordered levels)
    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Ensure we don't go out of bounds
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadSceneAsync(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels to load!");
        }
    }

    // The "Smart" Asynchronous Coroutine
    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        // This starts the loading process
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Optional: Prevent the scene from activating immediately
        // asyncLoad.allowSceneActivation = false; 

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            // Ideally, update a Loading Bar UI here using asyncLoad.progress
            yield return null;
        }
    }
}