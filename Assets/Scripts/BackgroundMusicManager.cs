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
            if (audioSource.clip == newClip)
                return;
                
            StartCoroutine(FadeToNewMusic(newClip));
        }
    }
    
    public void ChangeMusicByName(string musicName)
    {
        if (string.IsNullOrEmpty(musicName))
            return;
            
        AudioClip newClip = Resources.Load<AudioClip>("Music/" + musicName);
        if (newClip != null)
        {
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
        
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        audioSource.Stop();
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
}