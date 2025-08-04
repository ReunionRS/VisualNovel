using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VisualNovelManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image backgroundImage;
    public Image backgroundImageOverlay; // Второй слой для плавных переходов
    public Image dialogueBoxBackground; // Фон для текстового блока
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
    public AudioClip guitarClip;
    
    [Header("Name Input")]
    public NameInputManager nameInputManager;
    
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
        
        if (playerNameText != null)
            playerNameText.text = playerName;
        
        nextButton.onClick.AddListener(NextDialogue);
        
        // Настраиваем начальное состояние диалогового блока
        if (dialogueBoxBackground != null && !autoShowDialogueBox)
        {
            SetDialogueBoxVisible(false);
        }
        
        ShowNextDialogue();
    }
    
    public void NextDialogue()
    {
        if (isTyping)
        {
            // Пропустить анимацию печати
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
            // Конец главы
            EndChapter();
            return;
        }
        
        ShowNextDialogue();
    }
    
    void ShowNextDialogue()
    {
        if (currentDialogueIndex >= currentChapter.dialogues.Length) return;
        
        DialogueEntry currentEntry = currentChapter.dialogues[currentDialogueIndex];
        
        // Проверяем специальные команды в тексте
        if (currentEntry.text.Contains("[ВВОД_ИМЕНИ]"))
        {
            // Показываем окно ввода имени
            if (nameInputManager != null)
            {
                waitingForNameInput = true;
                nameInputManager.ShowNameInput();
                return; // Не показываем диалог, пока не введут имя
            }
        }
        
        // Загрузить фон
        LoadBackground(currentEntry.backgroundImage);
        
        // Воспроизвести звук
        if (!string.IsNullOrEmpty(currentEntry.soundEffect))
        {
            PlaySoundEffect(currentEntry.soundEffect);
        }
        
        // Обновить имя говорящего
        if (playerNameText != null)
        {
            string speakerName = ProcessText(currentEntry.speakerName);
            playerNameText.text = speakerName;
        }
        
        // Показать текст с эффектом печатной машинки
        string processedText = ProcessText(currentEntry.text);
        typingCoroutine = StartCoroutine(TypewriterEffect(processedText));
    }
    
    // Метод для продолжения после ввода имени
    public void ContinueAfterNameInput()
    {
        waitingForNameInput = false;
        
        // Пропускаем диалог с [ВВОД_ИМЕНИ] и переходим к следующему
        currentDialogueIndex++;
        if (currentDialogueIndex < currentChapter.dialogues.Length)
        {
            ShowNextDialogue();
        }
    }
    
    // Обработка специальных тегов в тексте
    string ProcessText(string text)
    {
        return text.Replace("[ИМЯ ИГРОКА]", playerName);
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
        
        // Если есть overlay слой, используем его для плавного перехода
        if (backgroundImageOverlay != null)
        {
            // Устанавливаем новый фон на overlay
            backgroundImageOverlay.sprite = newBackground;
            backgroundImageOverlay.color = new Color(1, 1, 1, 0);
            
            Debug.Log($"Fading over {backgroundFadeDuration} seconds");
            
            // Плавно показываем overlay
            float elapsedTime = 0;
            while (elapsedTime < backgroundFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(0, 1, elapsedTime / backgroundFadeDuration);
                backgroundImageOverlay.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            
            Debug.Log("Fade completed, swapping backgrounds");
            
            // Заменяем основной фон и скрываем overlay
            backgroundImage.sprite = newBackground;
            backgroundImageOverlay.color = new Color(1, 1, 1, 0);
        }
        else
        {
            // Простая замена без анимации, если нет overlay
            backgroundImage.sprite = newBackground;
            Debug.LogWarning("No overlay found, using instant background change");
        }
    }
    
    void PlaySoundEffect(string effectName)
    {
        if (sfxSource == null) return;
        
        switch (effectName)
        {
            case "guitar":
                if (guitarClip != null)
                {
                    sfxSource.clip = guitarClip;
                    sfxSource.Play();
                }
                break;
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
        // Показать текст завершения главы
        dialogueText.text = $"{currentChapter.chapterTitle} - завершена";
        if (playerNameText != null)
            playerNameText.text = "";
        
        // Подождать
        yield return new WaitForSeconds(3f);
        
        // Перейти к следующей главе, если есть
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
    
    // Управление видимостью диалогового блока
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
        
        // Также управляем текстом и кнопкой
        CanvasGroup dialogueGroup = dialoguePanel;
        if (dialogueGroup == null)
        {
            // Если нет CanvasGroup, управляем напрямую через альфа Image
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
            
            // Устанавливаем финальные значения
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
            // Используем CanvasGroup для плавного появления/исчезновения
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
    
    // Метод для показа диалогового блока при необходимости
    public void ShowDialogueBoxIfNeeded()
    {
        if (dialogueBoxBackground != null && dialogueBoxBackground.color.a < 0.5f)
        {
            SetDialogueBoxVisible(true);
        }
    }
}