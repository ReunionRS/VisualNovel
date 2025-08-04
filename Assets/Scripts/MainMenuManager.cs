using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject nameInputPanel;

    [Header("Main Menu Buttons")]
    public Button newGameButton;
    public Button loadGameButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Name Input UI")]
    public TMP_InputField nameInputField;
    public Button confirmNameButton;
    public Button backFromNameButton;
    public TextMeshProUGUI namePromptText;
    public TextMeshProUGUI nameErrorText;

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
        // Настраиваем кнопки главного меню
        if (newGameButton != null)
            newGameButton.onClick.AddListener(ShowNameInput);

        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        // Настраиваем кнопки ввода имени
        if (confirmNameButton != null)
            confirmNameButton.onClick.AddListener(ConfirmPlayerName);

        if (backFromNameButton != null)
            backFromNameButton.onClick.AddListener(BackFromNameInput);

        // Настраиваем поле ввода
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(OnNameInputChanged);
            nameInputField.onEndEdit.AddListener(OnNameInputSubmit);
        }
    }

    public void ShowNameInput()
    {
        // Скрываем главное меню
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
            
        // Показываем панель ввода имени
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            
            // Настраиваем текст подсказки
            if (namePromptText != null)
                namePromptText.text = "Придумайте имя для своего персонажа:";
                
            // Очищаем поле ввода и ошибки
            if (nameInputField != null)
            {
                nameInputField.text = "";
                var placeholder = nameInputField.placeholder as TextMeshProUGUI;
                if (placeholder != null)
                    placeholder.text = "Введите ваше имя...";
                    
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
            
            if (nameErrorText != null)
            {
                nameErrorText.text = "";
                nameErrorText.gameObject.SetActive(false);
            }
        }
    }

    private void OnNameInputChanged(string playerName)
    {
        // Скрываем ошибку при вводе
        if (nameErrorText != null && nameErrorText.gameObject.activeInHierarchy)
        {
            nameErrorText.gameObject.SetActive(false);
        }

        // Валидация в реальном времени
        ValidateNameInput(playerName, false);
    }

    private void OnNameInputSubmit(string playerName)
    {
        // Подтверждение имени при нажатии Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmPlayerName();
        }
    }

    private bool ValidateNameInput(string playerName, bool showError = true)
    {
        string trimmedName = playerName.Trim();

        // Проверка на пустое имя
        if (string.IsNullOrEmpty(trimmedName))
        {
            if (showError) ShowNameError("Пожалуйста, введите ваше имя");
            return false;
        }

        // Проверка на минимальную длину
        if (trimmedName.Length < 2)
        {
            if (showError) ShowNameError("Имя должно содержать минимум 2 символа");
            return false;
        }

        // Проверка на максимальную длину
        if (trimmedName.Length > 20)
        {
            if (showError) ShowNameError("Имя не должно превышать 20 символов");
            return false;
        }

        // Проверка на недопустимые символы
        foreach (char c in trimmedName)
        {
            if (!char.IsLetter(c) && !char.IsWhiteSpace(c) && c != '-' && c != '\'')
            {
                if (showError) ShowNameError("Имя может содержать только буквы, пробелы, дефисы и апострофы");
                return false;
            }
        }

        return true;
    }

    private void ShowNameError(string errorMessage)
    {
        if (nameErrorText != null)
        {
            nameErrorText.text = errorMessage;
            nameErrorText.gameObject.SetActive(true);
        }
    }

    public void ConfirmPlayerName()
    {
        string playerName = nameInputField != null ? nameInputField.text.Trim() : "";
        
        if (ValidateNameInput(playerName))
        {
            // Сохраняем имя игрока в PlayerPrefs
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.SetInt("GameStarted", 1);
            PlayerPrefs.Save();
            
            // Скрываем панель ввода имени
            if (nameInputPanel != null)
                nameInputPanel.SetActive(false);
            
            // Запускаем новую игру
            StartNewGame();
        }
    }

    public void BackFromNameInput()
    {
        // Скрываем панель ввода имени
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);

        // Показываем главное меню
        ShowMainMenu();
    }

    public void StartNewGame()
    {
        // Проверяем что имя сцены задано
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("Game scene name is not set!");
            return;
        }
        
        // Проверяем что сцена существует в Build Settings (только в редакторе)
        #if UNITY_EDITOR
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath(gameSceneName);
        if (sceneIndex < 0)
        {
            // Пробуем найти по имени без расширения
            string scenePath = "Assets/Scenes/" + gameSceneName + ".unity";
            sceneIndex = SceneUtility.GetBuildIndexByScenePath(scenePath);
        }
        
        if (sceneIndex < 0)
        {
            Debug.LogError($"Scene '{gameSceneName}' not found in Build Settings!");
            return;
        }
        #endif
        
        // Загружаем игровую сцену
        try
        {
            SceneManager.LoadScene(gameSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading scene: {e.Message}");
        }
    }

    public void LoadGame()
    {
        Debug.Log("Loading saved game...");
        
        // Здесь будет логика загрузки сохранения
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            // TODO: Загрузить данные сохранения
            SceneManager.LoadScene(gameSceneName);
        }
    }

    public void OpenSettings()
    {
        Debug.Log("Opening settings...");
        
        // Скрываем главное меню
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // Показываем настройки
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
        
        // Скрываем настройки
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Показываем главное меню
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
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