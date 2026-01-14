using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer introVideo; 

    [Header("Settings")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        // Subscribe to the event
        introVideo.loopPointReached += OnVideoFinished;
        introVideo.Play(); 
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Unsubscribe to avoid memory leaks
        introVideo.loopPointReached -= OnVideoFinished;
        
        // Load the game scene directly. 
        // Since GameUIController hides itself on start, the player will just see black 
        // until the assets are loaded, effectively acting as a seamless transition.
        SceneManager.LoadSceneAsync(gameSceneName);
    }
}