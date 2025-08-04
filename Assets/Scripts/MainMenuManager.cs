using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    
    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button exitButton;
    
    [Header("Settings")]
    public SettingsManager settingsManager;
    
    [Header("Scene Management")]
    public string gameSceneName = "GameScene";
    
    private void Start()
    {
        SetupButtons();
        ShowMainMenu();
    }
    
    private void SetupButtons()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);
            
        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
            
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }
    
    public void StartNewGame()
    {
        Debug.Log("Starting new game...");
        
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogWarning("Game scene name is not set!");
        }
    }
    
    public void LoadGame()
    {
        Debug.Log("Loading saved game...");
        
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
    
    public void OpenSettings()
    {
        Debug.Log("Opening settings...");
        
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        if (settingsManager != null)
        {
            settingsManager.ShowSettings();
        }
        else if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Settings manager or panel not assigned!");
        }
    }
    
    public void CloseSettings()
    {
        Debug.Log("Closing settings...");
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        ShowMainMenu();
    }
    
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}