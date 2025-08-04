using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Pause UI")]
    public GameObject pauseMenuPanel;
    public CanvasGroup pauseCanvasGroup;
    
    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button saveGameButton;
    public Button loadGameButton;
    public Button mainMenuButton;
    public Button quitGameButton;
    
    [Header("Settings Integration")]
    public SettingsManager settingsManager;
    public GameObject settingsPanel;
    
    [Header("Audio")]
    public AudioSource pauseSFX;
    public AudioClip pauseSound;
    public AudioClip unpauseSound;
    
    private bool isPaused = false;
    private float originalTimeScale = 1f;
    
    // Синглтон для доступа из других скриптов
    public static PauseManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        SetupButtons();
        
        // Скрываем меню паузы при старте
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }
    
    void Update()
    {
        // Обработка клавиши Escape для паузы
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
        
        // Дополнительные горячие клавиши
        if (Input.GetKeyDown(KeyCode.Space) && isPaused)
        {
            ResumeGame();
        }
    }
    
    private void SetupButtons()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
            
        if (saveGameButton != null)
            saveGameButton.onClick.AddListener(SaveGame);
            
        if (loadGameButton != null)
            loadGameButton.onClick.AddListener(LoadGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
        if (quitGameButton != null)
            quitGameButton.onClick.AddListener(QuitGame);
    }
    
    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f; // Останавливаем время в игре
        
        // Показываем меню паузы
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            StartCoroutine(FadeInPauseMenu());
        }
        
        // Воспроизводим звук паузы
        PlayPauseSound(pauseSound);
        
        Debug.Log("Game paused");
    }
    
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        isPaused = false;
        Time.timeScale = originalTimeScale; // Восстанавливаем время
        
        // Скрываем меню паузы
        if (pauseMenuPanel != null)
        {
            StartCoroutine(FadeOutPauseMenu());
        }
        
        // Воспроизводим звук снятия паузы
        PlayPauseSound(unpauseSound);
        
        Debug.Log("Game resumed");
    }
    
    public void OpenSettings()
    {
        if (settingsManager != null)
        {
            // Скрываем меню паузы
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
                
            // Показываем настройки
            settingsManager.ShowSettings();
        }
        else if (settingsPanel != null)
        {
            pauseMenuPanel.SetActive(false);
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Settings manager or panel not assigned!");
        }
    }
    
    public void CloseSettings()
    {
        Debug.Log("Settings closed, returning to pause menu");
        
        // Скрываем настройки
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        // Показываем меню паузы обратно
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        
        // НЕ снимаем паузу - остаемся в меню паузы
    }
    
    public void SaveGame()
    {
        Debug.Log("Saving game...");
        
        // Здесь будет логика сохранения игры
        // Можно интегрировать с SaveManager
        
        // Показать уведомление о сохранении
        StartCoroutine(ShowSaveMessage());
    }
    
    public void LoadGame()
    {
        Debug.Log("Loading game...");
        
        // Здесь будет логика загрузки игры
        // Можно интегрировать с SaveManager
        
        ResumeGame(); // Снимаем паузу после загрузки
    }
    
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        
        // Восстанавливаем время перед сменой сцены
        Time.timeScale = 1f;
        isPaused = false;
        
        // Загружаем главное меню
        SceneManager.LoadScene("Menu"); // Замени на имя твоей сцены меню
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void PlayPauseSound(AudioClip clip)
    {
        if (pauseSFX != null && clip != null)
        {
            pauseSFX.PlayOneShot(clip);
        }
    }
    
    IEnumerator FadeInPauseMenu()
    {
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            float fadeDuration = 0.3f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime; // Используем unscaledDeltaTime для работы при паузе
                pauseCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            
            pauseCanvasGroup.alpha = 1f;
        }
    }
    
    IEnumerator FadeOutPauseMenu()
    {
        if (pauseCanvasGroup != null)
        {
            float elapsedTime = 0f;
            float fadeDuration = 0.2f;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                pauseCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            
            pauseCanvasGroup.alpha = 0f;
        }
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }
    
    IEnumerator ShowSaveMessage()
    {
        // Здесь можно показать UI сообщение "Игра сохранена"
        Debug.Log("Game saved!");
        yield return new WaitForSecondsRealtime(1f); // Используем realtime для работы при паузе
    }
    
    // Публичные методы для проверки состояния
    public bool IsPaused()
    {
        return isPaused;
    }
    
    // Метод для принудительного снятия паузы (для cutscenes и т.д.)
    public void ForceResume()
    {
        if (isPaused)
        {
            ResumeGame();
        }
    }
    
    // Вызывается при уничтожении объекта
    void OnDestroy()
    {
        // Восстанавливаем время при выходе из сцены
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }
}