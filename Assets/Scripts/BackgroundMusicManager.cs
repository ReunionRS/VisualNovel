using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip backgroundMusic;
    
    [Header("Settings")]
    public float defaultVolume = 0.5f;
    private float currentVolume = 0.8f; // Громкость по умолчанию из GameSettings

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Загружаем настройки громкости
            LoadVolumeSettings();
            PlayMusic();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void PlayMusic()
    {
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = currentVolume;
            audioSource.Play();
            
            Debug.Log($"Background music started with volume: {currentVolume}");
        }
        else
        {
            Debug.LogWarning("AudioSource или AudioClip не назначен!");
        }
    }
    
    // Загружаем настройки громкости из PlayerPrefs
    void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey("GameSettings"))
        {
            string json = PlayerPrefs.GetString("GameSettings");
            GameSettings settings = JsonUtility.FromJson<GameSettings>(json);
            currentVolume = settings.musicVolume;
            Debug.Log($"Music volume loaded from settings: {currentVolume}");
        }
        else
        {
            currentVolume = defaultVolume;
            Debug.Log($"No settings found, using default volume: {currentVolume}");
        }
    }
    
    // Публичный метод для изменения громкости (вызывается из SettingsManager)
    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume); // Ограничиваем от 0 до 1
        
        if (audioSource != null)
        {
            audioSource.volume = currentVolume;
            Debug.Log($"Music volume changed to: {currentVolume}");
        }
    }
    
    // Метод для получения текущей громкости
    public float GetVolume()
    {
        return currentVolume;
    }
    
    // Метод для включения/выключения музыки
    public void SetMusicEnabled(bool enabled)
    {
        if (audioSource != null)
        {
            if (enabled && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
            else if (!enabled && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }
    
    // Метод для смены музыкального трека
    public void ChangeMusic(AudioClip newClip)
    {
        if (audioSource != null && newClip != null)
        {
            audioSource.Stop();
            audioSource.clip = newClip;
            audioSource.Play();
            backgroundMusic = newClip;
        }
    }
}