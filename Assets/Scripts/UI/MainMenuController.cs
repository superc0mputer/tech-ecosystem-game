using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button playButton;
    public Button resetSaveButton;
    
    // Set this to the exact name of your game scene in Build Settings
    public string gameSceneName = "GameScene"; 

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
            
            // Optional: Disable reset button if no save exists
            if(resetSaveButton) resetSaveButton.interactable = hasSave;
        }
    }

    // Connect to PLAY button
    public void OnPlayClicked()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Connect to RESET SAVE button
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