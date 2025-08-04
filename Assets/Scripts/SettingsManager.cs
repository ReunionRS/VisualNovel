using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class GameSettings
{
    [Header("Display Settings")]
    public bool isFullscreen = true;
    public bool skipRead = false;
    public bool skipAfterChoice = true;
    
    [Header("Speed Settings")]
    [Range(0.1f, 3f)]
    public float textSpeed = 1f;
    [Range(0.1f, 5f)]
    public float autoSpeed = 2f;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.8f;
    [Range(0f, 1f)]
    public float soundVolume = 0.8f;
    
    [Header("Language")]
    public SystemLanguage language = SystemLanguage.Russian;
}

public class SettingsManager : MonoBehaviour
{
    [Header("Settings UI")]
    public GameObject settingsPanel;
    public GameObject firstTimeSetupPanel;
    
    [Header("Display Settings")]
    public Toggle fullscreenToggle;
    public Toggle skipReadToggle;
    public Toggle skipAfterChoiceToggle;
    
    [Header("Speed Settings")]
    public Slider textSpeedSlider;
    public Slider autoSpeedSlider;
    
    [Header("Audio Settings")]
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;
    
    [Header("Language Settings")]
    public TMP_Dropdown languageDropdown;
    
    [Header("Buttons")]
    public Button okButton;
    public Button cancelButton;
    public Button defaultsButton;
    
    [Header("References")]
    public VisualNovelManager vnManager;
    public AudioSource musicSource;
    public AudioSource soundSource;
    public MainMenuManager mainMenuManager; // Добавляем ссылку на главное меню
    
    private GameSettings currentSettings;
    private GameSettings originalSettings; // Для отмены изменений
    
    private void Start()
    {
        LoadSettings();
        SetupUI();
        
        // Проверяем, первый ли раз запускается игра
        if (!PlayerPrefs.HasKey("FirstTimePlayed"))
        {
            ShowFirstTimeSetup();
        }
        else
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
        
        ApplySettings();
    }
    
    private void SetupUI()
    {
        // Настраиваем UI элементы
        if (okButton != null)
            okButton.onClick.AddListener(SaveAndClose);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelAndClose);
            
        if (defaultsButton != null)
            defaultsButton.onClick.AddListener(ResetToDefaults);
        
        // Настраиваем слайдеры
        if (textSpeedSlider != null)
            textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
            
        if (autoSpeedSlider != null)
            autoSpeedSlider.onValueChanged.AddListener(OnAutoSpeedChanged);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
        if (soundVolumeSlider != null)
            soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        
        // Настраиваем тогглы
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            
        if (skipReadToggle != null)
            skipReadToggle.onValueChanged.AddListener(OnSkipReadChanged);
            
        if (skipAfterChoiceToggle != null)
            skipAfterChoiceToggle.onValueChanged.AddListener(OnSkipAfterChoiceChanged);
        
        // Настраиваем dropdown языков
        if (languageDropdown != null)
        {
            languageDropdown.options.Clear();
            languageDropdown.options.Add(new TMP_Dropdown.OptionData("Русский"));
            languageDropdown.options.Add(new TMP_Dropdown.OptionData("English"));
            languageDropdown.options.Add(new TMP_Dropdown.OptionData("日本語"));
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
    }
    
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            // Сохраняем текущие настройки для возможности отмены
            originalSettings = JsonUtility.FromJson<GameSettings>(JsonUtility.ToJson(currentSettings));
            
            settingsPanel.SetActive(true);
            UpdateUI();
        }
    }
    
    public void ShowFirstTimeSetup()
    {
        if (firstTimeSetupPanel != null)
        {
            firstTimeSetupPanel.SetActive(true);
            UpdateUI();
        }
    }
    
    private void UpdateUI()
    {
        // Обновляем UI элементы в соответствии с текущими настройками
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = currentSettings.isFullscreen;
            
        if (skipReadToggle != null)
            skipReadToggle.isOn = currentSettings.skipRead;
            
        if (skipAfterChoiceToggle != null)
            skipAfterChoiceToggle.isOn = currentSettings.skipAfterChoice;
            
        if (textSpeedSlider != null)
            textSpeedSlider.value = currentSettings.textSpeed;
            
        if (autoSpeedSlider != null)
            autoSpeedSlider.value = currentSettings.autoSpeed;
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = currentSettings.musicVolume;
            
        if (soundVolumeSlider != null)
            soundVolumeSlider.value = currentSettings.soundVolume;
            
        if (languageDropdown != null)
        {
            int langIndex = currentSettings.language == SystemLanguage.Russian ? 0 : 
                           currentSettings.language == SystemLanguage.English ? 1 : 2;
            languageDropdown.value = langIndex;
        }
    }
    
    // Обработчики событий UI
    private void OnFullscreenChanged(bool value)
    {
        currentSettings.isFullscreen = value;
        Screen.fullScreen = value;
    }
    
    private void OnSkipReadChanged(bool value)
    {
        currentSettings.skipRead = value;
    }
    
    private void OnSkipAfterChoiceChanged(bool value)
    {
        currentSettings.skipAfterChoice = value;
    }
    
    private void OnTextSpeedChanged(float value)
    {
        currentSettings.textSpeed = value;
        if (vnManager != null)
            vnManager.typewriterSpeed = 0.1f / value; // Обратная зависимость
    }
    
    private void OnAutoSpeedChanged(float value)
    {
        currentSettings.autoSpeed = value;
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        currentSettings.musicVolume = value;
        if (musicSource != null)
            musicSource.volume = value;
    }
    
    private void OnSoundVolumeChanged(float value)
    {
        currentSettings.soundVolume = value;
        if (soundSource != null)
            soundSource.volume = value;
    }
    
    private void OnLanguageChanged(int value)
    {
        SystemLanguage[] languages = { SystemLanguage.Russian, SystemLanguage.English, SystemLanguage.Japanese };
        currentSettings.language = languages[value];
    }
    
    public void SaveAndClose()
    {
        SaveSettings();
        ApplySettings();
        CloseSettings();
        
        // Если это первый запуск, отмечаем это
        PlayerPrefs.SetInt("FirstTimePlayed", 1);
        PlayerPrefs.Save();
    }
    
    public void CancelAndClose()
    {
        // Восстанавливаем исходные настройки
        if (originalSettings != null)
        {
            currentSettings = JsonUtility.FromJson<GameSettings>(JsonUtility.ToJson(originalSettings));
            ApplySettings();
        }
        CloseSettings();
    }
    
    public void ResetToDefaults()
    {
        currentSettings = new GameSettings(); // Создаем настройки по умолчанию
        UpdateUI();
        ApplySettings();
    }
    
    private void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (firstTimeSetupPanel != null)
            firstTimeSetupPanel.SetActive(false);
            
        // Возвращаемся в главное меню, если мы в меню
        if (mainMenuManager != null)
        {
            mainMenuManager.ShowMainMenu();
        }
    }
    
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("GameSettings"))
        {
            string json = PlayerPrefs.GetString("GameSettings");
            currentSettings = JsonUtility.FromJson<GameSettings>(json);
        }
        else
        {
            currentSettings = new GameSettings(); // Настройки по умолчанию
        }
    }
    
    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(currentSettings);
        PlayerPrefs.SetString("GameSettings", json);
        PlayerPrefs.Save();
    }
    
    private void ApplySettings()
    {
        // Применяем настройки экрана
        Screen.fullScreen = currentSettings.isFullscreen;
        
        // Применяем аудио настройки
        if (musicSource != null)
            musicSource.volume = currentSettings.musicVolume;
            
        if (soundSource != null)
            soundSource.volume = currentSettings.soundVolume;
        
        // Применяем настройки скорости текста
        if (vnManager != null)
            vnManager.typewriterSpeed = 0.1f / currentSettings.textSpeed;
    }
    
    // Публичные методы для доступа к настройкам
    public GameSettings GetSettings()
    {
        return currentSettings;
    }
    
    public bool ShouldSkipRead()
    {
        return currentSettings.skipRead;
    }
    
    public bool ShouldSkipAfterChoice()
    {
        return currentSettings.skipAfterChoice;
    }
}