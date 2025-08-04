using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip backgroundMusic;
    
    [Header("Settings")]
    public float defaultVolume = 0.5f;
    private float currentVolume = 0.8f; 

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
        
        if (audioSource != null)
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
            audioSource.Stop();
            audioSource.clip = newClip;
            audioSource.Play();
            backgroundMusic = newClip;
        }
    }
}