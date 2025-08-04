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
        
        // Всегда скрываем настройки при старте
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (firstTimeSetupPanel != null)
            firstTimeSetupPanel.SetActive(false);
        
        // Проверяем, первый ли раз запускается игра
        if (!PlayerPrefs.HasKey("FirstTimePlayed"))
        {
            // Можно показать первоначальную настройку позже
            // ShowFirstTimeSetup();
        }
        
        ApplySettingsToGame();
    }
    
    private void SetupUI()
    {
        // Настраиваем UI элементы
        if (okButton != null)
        {
            okButton.onClick.RemoveAllListeners(); // Удаляем старые события
            okButton.onClick.AddListener(ApplySettingsButton); // Применить без закрытия
        }
            
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(CancelAndClose); // Вернуться в меню
        }
            
        if (defaultsButton != null)
        {
            defaultsButton.onClick.RemoveAllListeners();
            defaultsButton.onClick.AddListener(ResetToDefaults);
        }
            
        /*if (backButton != null)
            backButton.onClick.AddListener(GoBack);*/ // Временно отключаем
        
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
            languageDropdown.options.Add(new TMP_Dropdown.OptionData("Удмурт"));
            languageDropdown.options.Add(new TMP_Dropdown.OptionData("Татар"));
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
    }
    
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            // Загружаем актуальные настройки из PlayerPrefs
            LoadSettings();
            
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
        // Проверяем, что настройки загружены
        if (currentSettings == null)
        {
            Debug.LogError("currentSettings is null! Loading default settings.");
            currentSettings = new GameSettings();
        }
        
        // Добавляем проверки на null
        Debug.Log("UpdateUI called");
        
        // Обновляем UI элементы в соответствии с текущими настройками
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = currentSettings.isFullscreen;
        else
            Debug.LogWarning("fullscreenToggle is null!");
            
        if (skipReadToggle != null)
            skipReadToggle.isOn = currentSettings.skipRead;
        else
            Debug.LogWarning("skipReadToggle is null!");
            
        if (skipAfterChoiceToggle != null)
            skipAfterChoiceToggle.isOn = currentSettings.skipAfterChoice;
        else
            Debug.LogWarning("skipAfterChoiceToggle is null!");
            
        if (textSpeedSlider != null)
            textSpeedSlider.value = currentSettings.textSpeed;
        else
            Debug.LogWarning("textSpeedSlider is null!");
            
        if (autoSpeedSlider != null)
            autoSpeedSlider.value = currentSettings.autoSpeed;
        else
            Debug.LogWarning("autoSpeedSlider is null!");
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = currentSettings.musicVolume;
        else
            Debug.LogWarning("musicVolumeSlider is null!");
            
        if (soundVolumeSlider != null)
            soundVolumeSlider.value = currentSettings.soundVolume;
        else
            Debug.LogWarning("soundVolumeSlider is null!");
            
        if (languageDropdown != null)
        {
            int langIndex = currentSettings.language == SystemLanguage.Russian ? 0 : 
                           currentSettings.language == SystemLanguage.English ? 1 : 2;
            languageDropdown.value = langIndex;
        }
        else
            Debug.LogWarning("languageDropdown is null!");
    }
    
    // Обработчики событий UI
    private void OnFullscreenChanged(bool value)
    {
        currentSettings.isFullscreen = value;
        Screen.fullScreen = value;
        Debug.Log($"Fullscreen changed to: {value}");
    }
    
    private void OnSkipReadChanged(bool value)
    {
        currentSettings.skipRead = value;
        Debug.Log($"Skip read changed to: {value}");
    }
    
    private void OnSkipAfterChoiceChanged(bool value)
    {
        currentSettings.skipAfterChoice = value;
        Debug.Log($"Skip after choice changed to: {value}");
    }
    
    private void OnTextSpeedChanged(float value)
    {
        currentSettings.textSpeed = value;
        Debug.Log($"Text speed changed to: {value}");
        if (vnManager != null)
            vnManager.typewriterSpeed = 0.1f / value; // Обратная зависимость
    }
    
    private void OnAutoSpeedChanged(float value)
    {
        currentSettings.autoSpeed = value;
        // Debug.Log($"Auto speed changed to: {value}"); // Убираем спам
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        currentSettings.musicVolume = value;
        Debug.Log($"Music volume changed to: {value}");
        
        // Обновляем громкость в AudioSource
        if (musicSource != null)
            musicSource.volume = value;
            
        // Обновляем громкость в BackgroundMusicManager
        if (BackgroundMusicManager.instance != null)
            BackgroundMusicManager.instance.SetVolume(value);
    }
    
    private void OnSoundVolumeChanged(float value)
    {
        currentSettings.soundVolume = value;
        Debug.Log($"Sound volume changed to: {value}");
        if (soundSource != null)
            soundSource.volume = value;
    }
    
    private void OnLanguageChanged(int value)
    {
        SystemLanguage[] languages = { SystemLanguage.Russian, SystemLanguage.English, SystemLanguage.Japanese };
        currentSettings.language = languages[value];
        Debug.Log($"Language changed to: {currentSettings.language}");
    }
    
    public void SaveAndClose()
    {
        SaveSettingsToFile();
        ApplySettingsToGame();
        CloseSettings();
        
        // Если это первый запуск, отмечаем это
        PlayerPrefs.SetInt("FirstTimePlayed", 1);
        PlayerPrefs.Save();
    }
    
    // Метод для кнопки "Применить" - сохраняет БЕЗ закрытия
    public void ApplySettingsButton()
    {
        if (currentSettings == null)
        {
            Debug.LogError("currentSettings is null!");
            return;
        }
        
        Debug.Log("Applying settings...");
        SaveSettingsToFile();
        ApplySettingsToGame();
        
        // Показываем сообщение "Настройки применены"
        StartCoroutine(ShowAppliedMessage());
    }
    
    // Переименовали старый ApplySettings
    private void ApplySettingsToGame()
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
    
    // Корутина для показа сообщения
    IEnumerator ShowAppliedMessage()
    {
        // Здесь можно показать UI сообщение или просто лог
        Debug.Log("Настройки применены!");
        
        // Если хочешь показать UI сообщение:
        // Создай Text объект и покажи его на 2 секунды
        yield return new WaitForSeconds(2f);
        
        // Скрыть сообщение
    }
    
    public void CancelAndClose()
    {
        // Восстанавливаем исходные настройки
        if (originalSettings != null)
        {
            currentSettings = JsonUtility.FromJson<GameSettings>(JsonUtility.ToJson(originalSettings));
            ApplySettingsToGame();
        }
        CloseSettings();
    }
    
    public void ResetToDefaults()
    {
        currentSettings = new GameSettings(); // Создаем настройки по умолчанию
        UpdateUI();
        ApplySettingsToGame();
    }
    
    public void GoBack()
    {
        // Просто возвращаемся без сохранения изменений
        CancelAndClose();
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
            Debug.Log($"Settings loaded: autoSpeed = {currentSettings.autoSpeed}");
        }
        else
        {
            currentSettings = new GameSettings(); // Настройки по умолчанию
            Debug.Log("No saved settings found, using defaults");
        }
    }
    
    private void SaveSettingsToFile()
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