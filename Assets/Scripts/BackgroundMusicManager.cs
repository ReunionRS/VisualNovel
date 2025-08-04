using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip backgroundMusic;

    void Awake()
    {
        // Синглтон — чтобы музыка не обрывалась при смене сцен
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        PlayMusic();
    }

    void PlayMusic()
    {
        if (audioSource != null && backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 0.5f; // Громкость по вкусу
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource или AudioClip не назначен!");
        }
    }
}
