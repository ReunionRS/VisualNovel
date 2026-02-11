using UnityEngine;
using System.Collections;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip backgroundMusic;
    
    [Header("Settings")]
    public float defaultVolume = 0.5f;
    public float fadeSpeed = 1f; 
    private float currentVolume = 0.8f;
    
    private bool isFading = false;
    private string currentMusicName = ""; // Запоминаем текущую музыку

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
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
    
    public void SetVolume(float volume)
    {
        currentVolume = Mathf.Clamp01(volume);
        
        if (audioSource != null && !isFading)
        {
            audioSource.volume = currentVolume;
            Debug.Log($"Music volume changed to: {currentVolume}");
        }
    }
    
    public float GetVolume()
    {
        return currentVolume;
    }
    
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
    
    public void ChangeMusic(AudioClip newClip)
    {
        if (audioSource != null && newClip != null)
        {
            // Проверяем, что это не та же самая музыка, что уже играет
            if (audioSource.clip == newClip && audioSource.isPlaying)
            {
                Debug.Log($"Music is already playing: {newClip.name}");
                return;
            }
                
            StartCoroutine(FadeToNewMusic(newClip));
        }
    }
    
    public void ChangeMusicByName(string musicName)
    {
        if (string.IsNullOrEmpty(musicName))
            return;
            
        // Проверяем, не пытаемся ли запустить ту же самую музыку
        if (currentMusicName == musicName && audioSource != null && audioSource.isPlaying)
        {
            Debug.Log($"Music '{musicName}' is already playing");
            return;
        }
        
        AudioClip newClip = Resources.Load<AudioClip>("Music/" + musicName);
        if (newClip != null)
        {
            currentMusicName = musicName;
            ChangeMusic(newClip);
        }
        else
        {
            Debug.LogWarning($"Music file not found: Music/{musicName}");
        }
    }
    
    IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        isFading = true;
        
        // Если музыка уже играет, делаем fade out
        if (audioSource.isPlaying)
        {
            float startVolume = audioSource.volume;
            while (audioSource.volume > 0)
            {
                audioSource.volume -= startVolume * Time.deltaTime * fadeSpeed;
                yield return null;
            }
            
            audioSource.Stop();
        }
        
        audioSource.clip = newClip;
        backgroundMusic = newClip;
        audioSource.Play();
        
        while (audioSource.volume < currentVolume)
        {
            audioSource.volume += currentVolume * Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        audioSource.volume = currentVolume;
        isFading = false;
        
        Debug.Log($"Music changed to: {newClip.name}");
    }
    
    public void StopMusicWithFade()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic());
        }
    }
    
    IEnumerator FadeOutMusic()
    {
        isFading = true;
        float startVolume = audioSource.volume;
        
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        audioSource.Stop();
        audioSource.volume = currentVolume;
        isFading = false;
        currentMusicName = ""; // Сбрасываем название
        
        Debug.Log("Music stopped with fade");
    }
    
    public void ResumeMusicWithFade()
    {
        if (audioSource != null && !audioSource.isPlaying && audioSource.clip != null)
        {
            StartCoroutine(FadeInMusic());
        }
    }
    
    IEnumerator FadeInMusic()
    {
        isFading = true;
        audioSource.volume = 0;
        audioSource.Play();
        
        while (audioSource.volume < currentVolume)
        {
            audioSource.volume += currentVolume * Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        audioSource.volume = currentVolume;
        isFading = false;
        
        Debug.Log("Music resumed with fade");
    }
    
    // МЕТОД 1: Приглушить музыку для озвучки
    public void DuckMusicForVoice(float duckVolume = 0.3f, float duration = 0.3f)
    {
        StartCoroutine(DuckMusicCoroutine(duckVolume, duration));
    }
    
    IEnumerator DuckMusicCoroutine(float duckVolume, float duration)
    {
        if (!audioSource.isPlaying) yield break;
        
        float originalVolume = audioSource.volume;
        float targetVolume = originalVolume * duckVolume;
        
        // Плавно снижаем громкость
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            audioSource.volume = Mathf.Lerp(originalVolume, targetVolume, t);
            yield return null;
        }
        
        audioSource.volume = targetVolume;
    }
    
    // МЕТОД 2: Восстановить громкость музыки
    public void RestoreMusicVolume(float duration = 0.5f)
    {
        StartCoroutine(RestoreVolumeCoroutine(duration));
    }
    
    IEnumerator RestoreVolumeCoroutine(float duration)
    {
        if (!audioSource.isPlaying) yield break;
        
        float currentVol = audioSource.volume;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            audioSource.volume = Mathf.Lerp(currentVol, currentVolume, t);
            yield return null;
        }
        
        audioSource.volume = currentVolume;
    }
}