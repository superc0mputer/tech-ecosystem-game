using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button playButton;
    public Button resetSaveButton;
    
    [Header("Scene Configuration")]
    // Set this to the exact name of your game scene in Build Settings
    public string gameSceneName = "GameScene"; 
    
    // Set this to the exact name of your Intro Video scene in Build Settings
    public string introSceneName = "IntroScene";

    private void Start()
    {
        UpdateButtonsState();
    }

    private void UpdateButtonsState()
    {
        if (SaveManager.Instance != null)
        {
            bool hasSave = SaveManager.Instance.HasSaveFile();
            
            // Optional: Change Play text based on state
            // TextMeshProUGUI btnText = playButton.GetComponentInChildren<TextMeshProUGUI>();
            // if(btnText) btnText.text = hasSave ? "Continue" : "New Game";
            
            // Disable reset button if no save exists
            if(resetSaveButton) resetSaveButton.interactable = hasSave;
        }
    }

    // Connect this to your PLAY button in the Inspector
    public void OnPlayClicked()
    {
        // Check if the SaveManager exists and if we actually have a save file
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveFile())
        {
            // Case A: Save exists -> CONTINUE -> Go straight to Game
            Debug.Log("Save found. Skipping intro, loading Game.");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            // Case B: No Save -> NEW GAME -> Go to Intro Video
            Debug.Log("No save found. Loading Intro Video.");
            SceneManager.LoadScene(introSceneName);
        }
    }

    // Connect this to your RESET SAVE button in the Inspector
    public void OnResetSaveClicked()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSaveFile();
            UpdateButtonsState();
        }
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}