using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class WebGLVideoLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string videoFileName = "OpeningSequence.mp4"; // Make sure this matches your file name

    void Start()
    {
        // 1. Construct the path
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        
        // 2. Fix the path for WebGL (important!)
        // Application.streamingAssetsPath returns a path with backslashes on Windows, 
        // which breaks URLs in the browser. We must replace them.
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // WebGL expects a web URL, not a local file path
            // In WebGL, streamingAssetsPath is just the URL to the folder
            // We ensure we don't have backslashes
        }
        
        // General safe fix for URL formatting
        videoPlayer.url = videoPath;

        // 3. Prepare the video (don't play yet)
        videoPlayer.Prepare();
        
        // 4. Wait for user interaction to play (Browser requirement)
        // See the next section below
    }
    
    // Call this function from a UI Button
    public void PlayVideo()
    {
        if (videoPlayer.isPrepared)
        {
            videoPlayer.Play();
        }
    }
}