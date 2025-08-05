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
    
    [Header("Audio")]
    public string soundEffect;
    public string backgroundMusic;
    public string voiceClip;
    
    [Header("Character")]
    public string speakerName = "[ИМЯ ИГРОКА]";
}