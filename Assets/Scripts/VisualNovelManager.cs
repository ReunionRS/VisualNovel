using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VisualNovelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image backgroundImage;
    public Image backgroundImageOverlay;
    public Image dialogueBoxBackground;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI playerNameText;
    public Button nextButton;
    public CanvasGroup dialoguePanel;
    
    [Header("Settings")]
    public float typewriterSpeed = 0.05f;
    [Header("Animation Settings")]
    public float backgroundFadeDuration = 1f;
    [Header("UI Animation")]
    public float dialogueBoxFadeDuration = 0.5f;
    public bool autoShowDialogueBox = true;
    public string playerName = "Игрок";
    
    [Header("Chapter Data")]
    public ChapterData currentChapter;
    
    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource voiceSource;
    public AudioClip guitarClip;
    
    private bool waitingForNameInput = false;
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    
    void Start()
    {
        if (currentChapter == null)
        {
            Debug.LogError("No chapter data assigned!");
            return;
        }
        
        LoadPlayerName();
        
        if (playerNameText != null)
            playerNameText.text = playerName;
        
        nextButton.onClick.AddListener(NextDialogue);
        
        if (dialogueBoxBackground != null && !autoShowDialogueBox)
        {
            SetDialogueBoxVisible(false);
        }
        
        ShowNextDialogue();
    }
    
    private void LoadPlayerName()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            
            if (!string.IsNullOrEmpty(savedName) && savedName.Trim().Length >= 2)
            {
                playerName = savedName.Trim();
                Debug.Log($"Имя игрока загружено: {playerName}");
            }
            else
            {
                playerName = "Игрок";
                Debug.LogWarning("Сохраненное имя некорректно, используем дефолтное");
            }
            
            if (playerNameText != null)
                playerNameText.text = playerName;
        }
        else
        {
            playerName = "Игрок";
            Debug.Log("Сохраненное имя не найдено, используем: " + playerName);
        }
    }
    
    public void NextDialogue()
    {
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            dialogueText.text = ProcessText(currentChapter.dialogues[currentDialogueIndex].text);
            isTyping = false;
            return;
        }
        
        currentDialogueIndex++;
        
        if (currentDialogueIndex >= currentChapter.dialogues.Length)
        {
            EndChapter();
            return;
        }
        
        ShowNextDialogue();
    }
    
    void ShowNextDialogue()
    {
        if (currentDialogueIndex >= currentChapter.dialogues.Length) return;
        
        DialogueEntry currentEntry = currentChapter.dialogues[currentDialogueIndex];
        
        if (currentEntry.text.Contains("[ВВОД_ИМЕНИ]"))
        {
        }
        
        LoadBackground(currentEntry.backgroundImage);
        
        if (!string.IsNullOrEmpty(currentEntry.backgroundMusic))
        {
            ChangeBGMusic(currentEntry.backgroundMusic);
        }
        
        if (!string.IsNullOrEmpty(currentEntry.soundEffect))
        {
            PlaySoundEffect(currentEntry.soundEffect);
        }
        
        if (!string.IsNullOrEmpty(currentEntry.voiceClip))
        {
            PlayVoiceClip(currentEntry.voiceClip);
        }
        
        if (playerNameText != null)
        {
            string speakerName = ProcessText(currentEntry.speakerName);
            playerNameText.text = speakerName;
        }
        
        string processedText = ProcessText(currentEntry.text);
        typingCoroutine = StartCoroutine(TypewriterEffect(processedText));
    }
    
    public void ContinueAfterNameInput()
    {
        waitingForNameInput = false;
        
        currentDialogueIndex++;
        if (currentDialogueIndex < currentChapter.dialogues.Length)
        {
            ShowNextDialogue();
        }
    }
    
    string ProcessText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
            
        string processedText = text.Replace("[ИМЯ ИГРОКА]", playerName);
        processedText = processedText.Replace("[PLAYER_NAME]", playerName);
        processedText = processedText.Replace("[NAME]", playerName);
        
        return processedText;
    }
    
    void LoadBackground(string imageName)
    {
        if (string.IsNullOrEmpty(imageName)) return;
        
        Sprite backgroundSprite = Resources.Load<Sprite>("Backgrounds/" + imageName);
        if (backgroundSprite != null)
        {
            Debug.Log($"Loading background: {imageName}");
            if (backgroundImageOverlay != null)
            {
                Debug.Log("Starting fade transition");
                StartCoroutine(FadeToNewBackground(backgroundSprite));
            }
            else
            {
                Debug.LogWarning("BackgroundImageOverlay is null! Fade transition disabled.");
                backgroundImage.sprite = backgroundSprite;
            }
        }
        else
        {
            Debug.LogWarning($"Background image not found: {imageName}");
        }
    }
    
    IEnumerator FadeToNewBackground(Sprite newBackground)
    {
        Debug.Log("Fade transition started");
        
        if (backgroundImageOverlay != null)
        {
            backgroundImageOverlay.sprite = newBackground;
            backgroundImageOverlay.color = new Color(1, 1, 1, 0);
            
            Debug.Log($"Fading over {backgroundFadeDuration} seconds");
            
            float elapsedTime = 0;
            while (elapsedTime < backgroundFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, elapsedTime / backgroundFadeDuration);
                backgroundImageOverlay.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            
            Debug.Log("Fade completed, swapping backgrounds");
            
            backgroundImage.sprite = newBackground;
            backgroundImageOverlay.color = new Color(1, 1, 1, 0);
        }
        else
        {
            backgroundImage.sprite = newBackground;
            Debug.LogWarning("No overlay found, using instant background change");
        }
    }
    
    void PlaySoundEffect(string effectName)
    {
        if (sfxSource == null || string.IsNullOrEmpty(effectName)) return;
        
        AudioClip sfxClip = Resources.Load<AudioClip>("SFX/" + effectName);
        if (sfxClip != null)
        {
            sfxSource.PlayOneShot(sfxClip);
            Debug.Log($"Playing SFX: {effectName}");
        }
        else
        {
            Debug.LogWarning($"SFX file not found: SFX/{effectName}");
            
            switch (effectName)
            {
                case "guitar":
                    if (guitarClip != null)
                    {
                        sfxSource.PlayOneShot(guitarClip);
                    }
                    break;
            }
        }
    }
    
    void PlayVoiceClip(string voiceName)
    {
        if (voiceSource == null || string.IsNullOrEmpty(voiceName)) return;
        
        AudioClip voiceClip = Resources.Load<AudioClip>("Voice/" + voiceName);
        if (voiceClip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = voiceClip;
            voiceSource.Play();
            Debug.Log($"Playing voice: {voiceName}");
        }
        else
        {
            Debug.LogWarning($"Voice file not found: Voice/{voiceName}");
        }
    }
    
    void ChangeBGMusic(string musicName)
    {
        if (BackgroundMusicManager.instance != null)
        {
            BackgroundMusicManager.instance.ChangeMusicByName(musicName);
            Debug.Log($"Changing background music to: {musicName}");
        }
        else
        {
            Debug.LogWarning("BackgroundMusicManager instance not found!");
        }
    }
    
    IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
    }
    
    void EndChapter()
    {
        StartCoroutine(ShowChapterEnd());
    }
    
    IEnumerator ShowChapterEnd()
    {
        dialogueText.text = $"{currentChapter.chapterTitle} - завершена";
        if (playerNameText != null)
            playerNameText.text = "";
        
        yield return new WaitForSeconds(3f);
        
        if (currentChapter.nextChapter != null)
        {
            LoadNextChapter();
        }
        else
        {
            Debug.Log("Конец игры!");
        }
    }
    
    public void LoadNextChapter()
    {
        if (currentChapter.nextChapter != null)
        {
            currentChapter = currentChapter.nextChapter;
            currentDialogueIndex = 0;
            ShowNextDialogue();
        }
    }
    
    public void SetPlayerName(string newName)
    {
        playerName = newName;
        if (playerNameText != null && currentDialogueIndex < currentChapter.dialogues.Length)
        {
            playerNameText.text = ProcessText(currentChapter.dialogues[currentDialogueIndex].speakerName);
        }
    }
    
    public void SetDialogueBoxVisible(bool visible)
    {
        if (dialogueBoxBackground != null)
        {
            StartCoroutine(FadeDialogueBox(visible));
        }
    }
    
    IEnumerator FadeDialogueBox(bool fadeIn)
    {
        if (dialogueBoxBackground == null) yield break;
        
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        
        CanvasGroup dialogueGroup = dialoguePanel;
        if (dialogueGroup == null)
        {
            Color boxColor = dialogueBoxBackground.color;
            Color textColor = dialogueText.color;
            
            float elapsedTime = 0;
            while (elapsedTime < dialogueBoxFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / dialogueBoxFadeDuration);
                
                dialogueBoxBackground.color = new Color(boxColor.r, boxColor.g, boxColor.b, alpha);
                dialogueText.color = new Color(textColor.r, textColor.g, textColor.b, alpha);
                
                if (playerNameText != null)
                {
                    Color nameColor = playerNameText.color;
                    playerNameText.color = new Color(nameColor.r, nameColor.g, nameColor.b, alpha);
                }
                
                yield return null;
            }
            
            dialogueBoxBackground.color = new Color(boxColor.r, boxColor.g, boxColor.b, endAlpha);
            dialogueText.color = new Color(textColor.r, textColor.g, textColor.b, endAlpha);
            if (playerNameText != null)
            {
                Color nameColor = playerNameText.color;
                playerNameText.color = new Color(nameColor.r, nameColor.g, nameColor.b, endAlpha);
            }
        }
        else
        {
            float elapsedTime = 0;
            while (elapsedTime < dialogueBoxFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / dialogueBoxFadeDuration);
                dialogueGroup.alpha = alpha;
                yield return null;
            }
            
            dialogueGroup.alpha = endAlpha;
        }
    }
    
    public void ShowDialogueBoxIfNeeded()
    {
        if (dialogueBoxBackground != null && dialogueBoxBackground.color.a < 0.5f)
        {
            SetDialogueBoxVisible(true);
        }
    }
}