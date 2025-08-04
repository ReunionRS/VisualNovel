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
            if (nameInputManager != null)
            {
                waitingForNameInput = true;
                nameInputManager.ShowNameInput();
                return; 
            }
        }
        
        LoadBackground(currentEntry.backgroundImage);
        
        if (!string.IsNullOrEmpty(currentEntry.soundEffect))
        {
            PlaySoundEffect(currentEntry.soundEffect);
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