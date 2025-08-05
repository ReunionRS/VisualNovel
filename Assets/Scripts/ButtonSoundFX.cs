using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Button))]
public class ButtonSoundFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Sound Effects")]
    [SerializeField] private string hoverSoundName = "button_hover";
    [SerializeField] private string clickSoundName = "button_click";
    [SerializeField] private AudioSource audioSource;
    
    [Header("Hover Effects")]
    [SerializeField] private bool enableHoverScale = true;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float animationSpeed = 5f;
    
    [SerializeField] private bool enableHoverColor = true;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 1f, 1f);
    
    [Header("Click Effects")]
    [SerializeField] private bool enableClickScale = true;
    [SerializeField] private float clickScale = 0.95f;
    
    private Button button;
    private Image buttonImage;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovering = false;
    private bool isPressed = false;
    
    private Coroutine scaleCoroutine;
    private Coroutine colorCoroutine;
    
    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        
        originalScale = transform.localScale;
        if (buttonImage != null)
            originalColor = buttonImage.color;
        
        if (audioSource == null)
            audioSource = FindObjectOfType<AudioSource>();
            
        if (enableHoverColor && buttonImage != null)
            normalColor = buttonImage.color;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovering = true;
        PlayHoverSound();
        
        if (enableHoverScale && !isPressed)
            AnimateScale(hoverScale);
            
        if (enableHoverColor && buttonImage != null)
            AnimateColor(hoverColor);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovering = false;
        
        if (enableHoverScale && !isPressed)
            AnimateScale(1f);
            
        if (enableHoverColor && buttonImage != null)
            AnimateColor(normalColor);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = true;
        PlayClickSound();
        
        if (enableClickScale)
            AnimateScale(clickScale);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = false;
        
        if (enableClickScale)
        {
            float targetScale = isHovering ? hoverScale : 1f;
            AnimateScale(targetScale);
        }
    }
    
    private void PlayHoverSound()
    {
        PlaySound(hoverSoundName);
    }
    
    private void PlayClickSound()
    {
        PlaySound(clickSoundName);
    }
    
    private void PlaySound(string soundName)
    {
        if (string.IsNullOrEmpty(soundName) || audioSource == null) return;
        
        AudioClip clip = Resources.Load<AudioClip>("SFX/UI/" + soundName);
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            clip = Resources.Load<AudioClip>("SFX/" + soundName);
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
    
    private void AnimateScale(float targetScale)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
            
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale));
    }
    
    private void AnimateColor(Color targetColor)
    {
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);
            
        colorCoroutine = StartCoroutine(ColorCoroutine(targetColor));
    }
    
    private IEnumerator ScaleCoroutine(float targetScale)
    {
        Vector3 target = originalScale * targetScale;
        
        while (Vector3.Distance(transform.localScale, target) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * animationSpeed);
            yield return null;
        }
        
        transform.localScale = target;
    }
    
    private IEnumerator ColorCoroutine(Color targetColor)
    {
        if (buttonImage == null) yield break;
        
        Color startColor = buttonImage.color;
        
        while (Vector4.Distance(buttonImage.color, targetColor) > 0.01f)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.unscaledDeltaTime * animationSpeed);
            yield return null;
        }
        
        buttonImage.color = targetColor;
    }
    
    public void SetSounds(string hoverSound, string clickSound)
    {
        hoverSoundName = hoverSound;
        clickSoundName = clickSound;
    }
    
    public void SetHoverScale(float scale)
    {
        hoverScale = scale;
    }
    
    public void SetClickScale(float scale)
    {
        clickScale = scale;
    }
    
    public void SetHoverColor(Color color)
    {
        hoverColor = color;
    }
}