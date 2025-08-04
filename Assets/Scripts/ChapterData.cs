using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Chapter", menuName = "Visual Novel/Chapter Data")]
public class ChapterData : ScriptableObject
{
    [Header("Chapter Info")]
    public string chapterTitle;
    public int chapterNumber;
    
    [Header("Dialogue")]
    public DialogueEntry[] dialogues;
    
    [Header("Next Chapter")]
    public ChapterData nextChapter;
}