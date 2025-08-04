using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    [TextArea(3, 10)]
    public string text;
    public string backgroundImage;
    public string soundEffect;
    [Header("Character")]
    public string speakerName = "[ИМЯ ИГРОКА]"; // По умолчанию главный герой
}