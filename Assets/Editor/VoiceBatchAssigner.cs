#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Утилита для пакетного назначения Voice Clips всем главам
/// </summary>
public class VoiceBatchAssigner
{
    [MenuItem("Tools/Visual Novel/Batch Assign Voice to All Chapters", false, 42)]
    public static void BatchAssignVoiceClips()
    {
        if (!EditorUtility.DisplayDialog(
            "Batch Voice Assignment",
            "Автоматически назначить Voice Clip Direct всем главам?\n\n" +
            "Это найдёт все ChapterData в проекте и попытается назначить AudioClip'ы " +
            "на основе Voice Clip Name.",
            "Yes", "Cancel"))
        {
            return;
        }
        
        string[] guids = AssetDatabase.FindAssets("t:ChapterData");
        int totalChapters = 0;
        int totalAssigned = 0;
        int totalNotFound = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
            
            if (chapter != null && chapter.dialogues != null)
            {
                totalChapters++;
                Debug.Log($"Processing chapter: {chapter.chapterTitle}");
                
                foreach (var dialogue in chapter.dialogues)
                {
                    // Пропускаем если уже есть Direct clip
                    if (dialogue.voiceClipDirect != null)
                        continue;
                    
                    // Если есть имя клипа
                    if (!string.IsNullOrEmpty(dialogue.voiceClipName))
                    {
                        AudioClip foundClip = FindVoiceClip(dialogue.voiceClipName);
                        
                        if (foundClip != null)
                        {
                            dialogue.voiceClipDirect = foundClip;
                            totalAssigned++;
                        }
                        else
                        {
                            totalNotFound++;
                            Debug.LogWarning($"[{chapter.chapterTitle}] Voice not found: {dialogue.voiceClipName}");
                        }
                    }
                }
                
                EditorUtility.SetDirty(chapter);
            }
        }
        
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog(
            "Batch Assignment Complete",
            $"Processed Chapters: {totalChapters}\n" +
            $"Voice Clips Assigned: {totalAssigned}\n" +
            $"Not Found: {totalNotFound}",
            "OK"
        );
        
        Debug.Log($"Batch assignment complete: {totalChapters} chapters, {totalAssigned} assigned, {totalNotFound} not found");
    }
    
    static AudioClip FindVoiceClip(string clipName)
    {
        // Убираем расширение если есть
        clipName = Path.GetFileNameWithoutExtension(clipName);
        
        // Сначала пробуем загрузить напрямую
        AudioClip clip = Resources.Load<AudioClip>("Voice/" + clipName);
        if (clip != null) return clip;
        
        // Ищем во вложенных папках
        string[] commonFolders = { "HuTao", "Rex", "NPC", "Narrator", "Male", "Female" };
        
        foreach (string folder in commonFolders)
        {
            clip = Resources.Load<AudioClip>($"Voice/{folder}/{clipName}");
            if (clip != null) return clip;
        }
        
        // Если не нашли через Resources.Load, ищем в файловой системе
        string voicePath = Path.Combine(Application.dataPath, "Resources", "Voice");
        
        if (Directory.Exists(voicePath))
        {
            string[] extensions = { ".wav", ".mp3", ".ogg" };
            
            foreach (string ext in extensions)
            {
                string[] files = Directory.GetFiles(voicePath, $"{clipName}{ext}", SearchOption.AllDirectories);
                
                if (files.Length > 0)
                {
                    string relativePath = files[0].Replace(Application.dataPath, "Assets");
                    clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relativePath);
                    
                    if (clip != null) return clip;
                }
            }
        }
        
        return null;
    }
    
    [MenuItem("Tools/Visual Novel/Clear All Voice Direct Clips", false, 43)]
    public static void ClearAllDirectClips()
    {
        if (!EditorUtility.DisplayDialog(
            "Clear All Direct Clips",
            "Удалить Voice Clip Direct из ВСЕХ глав?\n\n" +
            "Voice Clip Name останутся без изменений.",
            "Yes", "Cancel"))
        {
            return;
        }
        
        string[] guids = AssetDatabase.FindAssets("t:ChapterData");
        int cleared = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
            
            if (chapter != null && chapter.dialogues != null)
            {
                foreach (var dialogue in chapter.dialogues)
                {
                    if (dialogue.voiceClipDirect != null)
                    {
                        dialogue.voiceClipDirect = null;
                        cleared++;
                    }
                }
                
                EditorUtility.SetDirty(chapter);
            }
        }
        
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog(
            "Clear Complete",
            $"Cleared {cleared} voice direct clips",
            "OK"
        );
        
        Debug.Log($"Cleared {cleared} voice direct clips from all chapters");
    }
}
#endif