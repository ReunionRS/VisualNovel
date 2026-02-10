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
    
    [Header("Character Sprite Layer")]
    public Image characterImage; // Основной слой персонажа
    public Image characterImageOverlay; // Оверлей для плавных переходов
    public CanvasGroup characterCanvasGroup; // Для управления прозрачностью
    
    [Header("Settings")]
    public float typewriterSpeed = 0.05f;
    
    [Header("Animation Settings")]
    public float backgroundFadeDuration = 1f;
    public float characterFadeDuration = 0.5f;
    
    [Header("Voice Sync Settings")]
    public bool enableVoiceSync = true;
    public float voiceSyncDelay = 0.1f; // Задержка перед началом озвучки
    public float voiceCharacterDelay = 0.03f; // Задержка между символами при озвучке
    
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
    private Coroutine voiceCoroutine;
    private string currentCharacterSprite = "";
    
    void Start()
    {
        if (currentChapter == null)
        {
            Debug.LogError("No chapter data assigned!");
            return;
        }
        
        // Инициализация слоя персонажа
        if (characterImage != null)
        {
            characterImage.gameObject.SetActive(true);
            if (characterCanvasGroup != null)
                characterCanvasGroup.alpha = 0f;
        }
        
        if (characterImageOverlay != null)
        {
            characterImageOverlay.gameObject.SetActive(true);
            characterImageOverlay.color = new Color(1, 1, 1, 0);
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
            // Прерываем печатание и озвучку
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            
            if (voiceCoroutine != null)
            {
                StopCoroutine(voiceCoroutine);
            }
            
            if (voiceSource != null && voiceSource.isPlaying)
            {
                voiceSource.Stop();
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
        
        // Загружаем фон
        LoadBackground(currentEntry.backgroundImage);
        
        // Загружаем/обновляем персонажа
        LoadCharacter(currentEntry);
        
        // Обрабатываем музыку
        if (!string.IsNullOrEmpty(currentEntry.backgroundMusic))
        {
            ChangeBGMusic(currentEntry.backgroundMusic);
        }
        
        // Обрабатываем звуковые эффекты
        if (!string.IsNullOrEmpty(currentEntry.soundEffect))
        {
            PlaySoundEffect(currentEntry.soundEffect);
        }
        
        // Обновляем имя говорящего
        if (playerNameText != null)
        {
            string speakerName = ProcessText(currentEntry.speakerName);
            playerNameText.text = speakerName;
        }
        
        // Запускаем текст и озвучку
        string processedText = ProcessText(currentEntry.text);
        
        if (currentEntry.HasVoice() && currentEntry.syncVoiceWithText && enableVoiceSync)
        {
            // Синхронизированная озвучка с текстом
            AudioClip voiceClip = currentEntry.GetVoiceClip();
            typingCoroutine = StartCoroutine(TypewriterEffectWithVoice(processedText, voiceClip));
        }
        else
        {
            // Обычная озвучка без синхронизации
            if (currentEntry.HasVoice())
            {
                AudioClip voiceClip = currentEntry.GetVoiceClip();
                PlayVoiceClipDirect(voiceClip);
            }
            typingCoroutine = StartCoroutine(TypewriterEffect(processedText));
        }
    }
    
    void LoadCharacter(DialogueEntry entry)
    {
        if (characterImage == null) return;
        
        // Если нужно скрыть персонажа
        if (entry.hideCharacter)
        {
            StartCoroutine(FadeCharacter(false));
            currentCharacterSprite = "";
            return;
        }
        
        // Если спрайт не указан, пропускаем
        if (string.IsNullOrEmpty(entry.characterSprite))
            return;
        
        // Загружаем спрайт персонажа
        Sprite characterSprite = Resources.Load<Sprite>("Characters/" + entry.characterSprite);
        if (characterSprite != null)
        {
            Debug.Log($"Loading character sprite: {entry.characterSprite}");
            
            // Если это новый персонаж, делаем плавный переход
            if (currentCharacterSprite != entry.characterSprite)
            {
                if (characterImageOverlay != null && !string.IsNullOrEmpty(currentCharacterSprite))
                {
                    StartCoroutine(FadeToNewCharacter(characterSprite));
                }
                else
                {
                    characterImage.sprite = characterSprite;
                    StartCoroutine(FadeCharacter(true));
                }
                currentCharacterSprite = entry.characterSprite;
            }
        }
        else
        {
            Debug.LogWarning($"Character sprite not found: Characters/{entry.characterSprite}");
        }
    }
    
    IEnumerator FadeCharacter(bool fadeIn)
    {
        if (characterCanvasGroup == null) yield break;
        
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < characterFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / characterFadeDuration);
            characterCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        characterCanvasGroup.alpha = endAlpha;
    }
    
    IEnumerator FadeToNewCharacter(Sprite newCharacter)
    {
        if (characterImageOverlay == null) yield break;
        
        // Устанавливаем новый спрайт на оверлей
        characterImageOverlay.sprite = newCharacter;
        characterImageOverlay.color = new Color(1, 1, 1, 0);
        
        float elapsedTime = 0f;
        while (elapsedTime < characterFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsedTime / characterFadeDuration);
            characterImageOverlay.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        
        // Переносим спрайт на основной слой
        characterImage.sprite = newCharacter;
        characterImageOverlay.color = new Color(1, 1, 1, 0);
        
        // Убеждаемся что персонаж виден
        if (characterCanvasGroup != null)
            characterCanvasGroup.alpha = 1f;
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
    
    void PlayVoiceClipDirect(AudioClip voiceClip)
    {
        if (voiceSource == null || voiceClip == null) return;
        
        voiceSource.Stop();
        voiceSource.clip = voiceClip;
        voiceSource.Play();
        Debug.Log($"Playing voice: {voiceClip.name}");
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
    
    IEnumerator TypewriterEffectWithVoice(string text, AudioClip voiceClip)
    {
        isTyping = true;
        dialogueText.text = "";
        
        // Проверяем аудиоклип
        if (voiceClip == null)
        {
            Debug.LogWarning("Voice clip is null, falling back to normal typewriter");
            yield return TypewriterEffect(text);
            yield break;
        }
        
        // Запускаем озвучку с небольшой задержкой
        yield return new WaitForSeconds(voiceSyncDelay);
        
        if (voiceSource != null)
        {
            voiceSource.Stop();
            voiceSource.clip = voiceClip;
            voiceSource.Play();
            Debug.Log($"Playing synced voice: {voiceClip.name}");
        }
        
        // Рассчитываем скорость печати на основе длины аудио и текста
        float audioDuration = voiceClip.length;
        int textLength = text.Length;
        float charDelay = (audioDuration / textLength) * 0.95f; // 95% для небольшого запаса
        
        // Минимальная и максимальная задержка между символами
        charDelay = Mathf.Clamp(charDelay, 0.02f, 0.15f);
        
        Debug.Log($"Voice sync: Audio duration: {audioDuration}s, Text length: {textLength}, Char delay: {charDelay}s");
        
        // Печатаем текст синхронно с озвучкой
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(charDelay);
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
        
        // Скрываем персонажа
        if (characterCanvasGroup != null)
        {
            yield return FadeCharacter(false);
        }
        
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
            currentCharacterSprite = "";
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