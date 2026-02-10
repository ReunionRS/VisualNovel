using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    [TextArea(3, 10)]
    public string text;
    
    [Header("Visuals")]
    public string backgroundImage;
    
    [Header("Character Sprite")]
    [Tooltip("Имя спрайта персонажа из Resources/Characters/")]
    public string characterSprite;
    
    [Tooltip("Скрыть персонажа на этой реплике")]
    public bool hideCharacter = false;
    
    [Header("Audio")]
    public string soundEffect;
    public string backgroundMusic;
    
    [Header("Voice Settings")]
    [Tooltip("Перетащите AudioClip сюда (имеет приоритет над Voice Clip Name)")]
    public AudioClip voiceClipDirect;
    
    [Tooltip("ИЛИ укажите имя файла из Resources/Voice/ (без расширения)")]
    public string voiceClipName;
    
    [Space(5)]
    [Tooltip("Синхронизировать скорость печати текста с длительностью озвучки")]
    public bool syncVoiceWithText = true;
    
    [Header("Character")]
    [Tooltip("Имя говорящего персонажа (используйте [ИМЯ ИГРОКА] для автозамены)")]
    public string speakerName = "[ИМЯ ИГРОКА]";
    
    // Вспомогательный метод для получения AudioClip
    public AudioClip GetVoiceClip()
    {
        // Приоритет: прямая ссылка > загрузка по имени
        if (voiceClipDirect != null)
            return voiceClipDirect;
            
        if (!string.IsNullOrEmpty(voiceClipName))
            return Resources.Load<AudioClip>("Voice/" + voiceClipName);
            
        return null;
    }
    
    // Проверка, есть ли озвучка
    public bool HasVoice()
    {
        return voiceClipDirect != null || !string.IsNullOrEmpty(voiceClipName);
    }
}