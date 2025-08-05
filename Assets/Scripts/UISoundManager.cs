using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }
    
    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    
    [Header("Default Sounds")]
    [SerializeField] private string defaultHoverSound = "button_hover";
    [SerializeField] private string defaultClickSound = "button_click";
    [SerializeField] private string defaultMenuOpenSound = "menu_open";
    [SerializeField] private string defaultMenuCloseSound = "menu_close";
    
    [Header("Volume")]
    [SerializeField] private float uiVolume = 0.7f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadVolumeSettings();
    }
    
    private void SetupAudioSource()
    {
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        uiAudioSource.volume = uiVolume;
        uiAudioSource.loop = false;
        uiAudioSource.playOnAwake = false;
    }
    
    private void LoadVolumeSettings()
    {
        if (PlayerPrefs.HasKey("GameSettings"))
        {
            string json = PlayerPrefs.GetString("GameSettings");
            GameSettings settings = JsonUtility.FromJson<GameSettings>(json);
            SetUIVolume(settings.soundVolume);
        }
    }
    
    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        if (uiAudioSource != null)
        {
            uiAudioSource.volume = uiVolume;
        }
    }
    
    public void PlayButtonHover()
    {
        PlayUISound(defaultHoverSound);
    }
    
    public void PlayButtonClick()
    {
        PlayUISound(defaultClickSound);
    }
    
    public void PlayMenuOpen()
    {
        PlayUISound(defaultMenuOpenSound);
    }
    
    public void PlayMenuClose()
    {
        PlayUISound(defaultMenuCloseSound);
    }
    
    public void PlayUISound(string soundName)
    {
        if (string.IsNullOrEmpty(soundName) || uiAudioSource == null) return;
        
        AudioClip clip = LoadUISound(soundName);
        if (clip != null)
        {
            uiAudioSource.PlayOneShot(clip, uiVolume);
        }
    }
    
    private AudioClip LoadUISound(string soundName)
    {
        AudioClip clip = Resources.Load<AudioClip>("SFX/UI/" + soundName);
        if (clip == null)
        {
            clip = Resources.Load<AudioClip>("SFX/" + soundName);
        }
        return clip;
    }
    
    public AudioSource GetUIAudioSource()
    {
        return uiAudioSource;
    }
}