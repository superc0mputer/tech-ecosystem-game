using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    [Header("References")]
    public VideoPlayer videoPlayer; // Drag your Video Player here in Inspector

    [Header("Settings")]
    public string gameSceneName = "GameScene";

    private void Start()
    {
        // Subscribe to the event that fires when the video finishes
        videoPlayer.loopPointReached += OnVideoFinished;
        
        // Optional: Ensure video plays immediately
        videoPlayer.Play(); 
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        LoadGameScene();
    }
    

    private void LoadGameScene()
    {
        // Unsubscribe to prevent memory leaks
        videoPlayer.loopPointReached -= OnVideoFinished;
        SceneManager.LoadScene(gameSceneName);
    }
}