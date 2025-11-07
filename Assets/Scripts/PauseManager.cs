using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel; // Assign or auto-find at runtime
    public GameObject mobileControls; // 🔹 Assign your MobileControlsCanvas here

    private bool isPaused = false;

    void Start()
    {
        // Make sure the game starts running normally
        Time.timeScale = 1f;

        // Auto-find PausePanel if not assigned
        if (pausePanel == null)
            pausePanel = GameObject.Find("PausePanel");

        // Hide the pause menu at start
        if (pausePanel != null)
            pausePanel.SetActive(false);
        else
            Debug.LogWarning("PauseManager: No PausePanel found in the scene!");

        // Make sure mobile controls are visible at start
        if (mobileControls == null)
            mobileControls = GameObject.Find("MobileControlsCanvas");
        if (mobileControls != null)
            mobileControls.SetActive(true);
    }

    // Called by the Pause button
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // Stop the game and show menu
    public void PauseGame()
    {
        Debug.Log("Game Paused");
        Time.timeScale = 0f; // Freeze gameplay
        isPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (mobileControls != null)
            mobileControls.SetActive(false); // 🔹 Hide buttons when paused
    }

    // Resume gameplay and hide menu
    public void ResumeGame()
    {
        Debug.Log("Resume button clicked");
        Time.timeScale = 1f; // Resume normal speed
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (mobileControls != null)
            mobileControls.SetActive(true); // 🔹 Show buttons again
    }

    // Quit to lobby
    public void QuitToLobby()
    {
        Debug.Log("Quit button clicked");
        Time.timeScale = 1f; // Unpause before switching scenes
        string lobbySceneName = "LobbyScene"; // Make sure this matches your real lobby scene name exactly!

        // Show mobile controls again for next scene
        if (mobileControls != null)
            mobileControls.SetActive(true);

        // Check if the scene exists in Build Settings
        if (Application.CanStreamedLevelBeLoaded(lobbySceneName))
        {
            SceneManager.LoadScene(lobbySceneName);
        }
        else
        {
            Debug.LogError($"PauseManager: Scene '{lobbySceneName}' not found in Build Settings!");
        }
    }
}
