#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Утилита для автоматического назначения Voice Clip Direct из папки Resources/Voice
/// </summary>
public class VoiceClipAutoAssigner : EditorWindow
{
    private ChapterData targetChapter;
    private bool convertToDirectClips = true;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Visual Novel/Voice Clip Auto Assigner", false, 41)]
    public static void ShowWindow()
    {
        VoiceClipAutoAssigner window = GetWindow<VoiceClipAutoAssigner>("Voice Auto Assigner");
        window.minSize = new Vector2(500, 400);
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Voice Clip Auto Assigner", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Эта утилита автоматически найдёт AudioClip'ы в Resources/Voice и назначит их в Voice Clip Direct.\n" +
            "Это решит проблему с билдом, где Resources.Load() не всегда работает корректно.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        targetChapter = (ChapterData)EditorGUILayout.ObjectField(
            "Target Chapter",
            targetChapter,
            typeof(ChapterData),
            false
        );
        
        if (targetChapter == null)
        {
            EditorGUILayout.HelpBox("Выберите ChapterData для работы", MessageType.Warning);
            return;
        }
        
        EditorGUILayout.Space();
        
        convertToDirectClips = EditorGUILayout.Toggle(
            "Convert Name → Direct",
            convertToDirectClips
        );
        
        EditorGUILayout.HelpBox(
            convertToDirectClips
                ? "Все Voice Clip Name будут преобразованы в Voice Clip Direct"
                : "Voice Clip Name останутся без изменений",
            MessageType.None
        );
        
        EditorGUILayout.Space();
        
        // Информация о главе
        GUILayout.Label("Chapter Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Chapter Title:", targetChapter.chapterTitle);
        EditorGUILayout.LabelField("Dialogue Count:", targetChapter.dialogues.Length.ToString());
        
        int withDirect = 0;
        int withName = 0;
        int withoutVoice = 0;
        
        foreach (var dialogue in targetChapter.dialogues)
        {
            if (dialogue.voiceClipDirect != null)
                withDirect++;
            else if (!string.IsNullOrEmpty(dialogue.voiceClipName))
                withName++;
            else
                withoutVoice++;
        }
        
        EditorGUILayout.LabelField("With Direct Clip:", withDirect.ToString());
        EditorGUILayout.LabelField("With Name Only:", withName.ToString());
        EditorGUILayout.LabelField("Without Voice:", withoutVoice.ToString());
        
        EditorGUILayout.Space();
        
        // Кнопка действия
        if (GUILayout.Button("Auto Assign Voice Clips", GUILayout.Height(40)))
        {
            AutoAssignVoiceClips();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Clear All Direct Clips", GUILayout.Height(25)))
        {
            ClearDirectClips();
        }
        
        EditorGUILayout.Space();
        
        // Список диалогов
        GUILayout.Label("Dialogues Preview", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < targetChapter.dialogues.Length; i++)
        {
            DialogueEntry dialogue = targetChapter.dialogues[i];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Dialogue {i}", EditorStyles.boldLabel);
            
            string preview = dialogue.text;
            if (preview.Length > 40)
                preview = preview.Substring(0, 40) + "...";
            EditorGUILayout.LabelField("Text:", preview);
            
            EditorGUILayout.LabelField("Direct:", dialogue.voiceClipDirect != null ? dialogue.voiceClipDirect.name : "None");
            EditorGUILayout.LabelField("Name:", !string.IsNullOrEmpty(dialogue.voiceClipName) ? dialogue.voiceClipName : "None");
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    void AutoAssignVoiceClips()
    {
        int assigned = 0;
        int notFound = 0;
        
        foreach (var dialogue in targetChapter.dialogues)
        {
            // Если уже есть Direct clip, пропускаем
            if (dialogue.voiceClipDirect != null && !convertToDirectClips)
                continue;
            
            // Если есть имя клипа
            if (!string.IsNullOrEmpty(dialogue.voiceClipName))
            {
                AudioClip foundClip = FindVoiceClip(dialogue.voiceClipName);
                
                if (foundClip != null)
                {
                    dialogue.voiceClipDirect = foundClip;
                    assigned++;
                    Debug.Log($"Assigned: {foundClip.name} → Dialogue {System.Array.IndexOf(targetChapter.dialogues, dialogue)}");
                }
                else
                {
                    notFound++;
                    Debug.LogWarning($"Voice clip not found: {dialogue.voiceClipName}");
                }
            }
        }
        
        EditorUtility.SetDirty(targetChapter);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog(
            "Auto Assignment Complete",
            $"Assigned: {assigned}\nNot Found: {notFound}",
            "OK"
        );
    }
    
    AudioClip FindVoiceClip(string clipName)
    {
        // Убираем расширение если есть
        clipName = Path.GetFileNameWithoutExtension(clipName);
        
        // Сначала пробуем загрузить напрямую
        AudioClip clip = Resources.Load<AudioClip>("Voice/" + clipName);
        if (clip != null) return clip;
        
        // Ищем во вложенных папках
        string[] folders = { "HuTao", "Rex", "NPC", "Narrator" };
        
        foreach (string folder in folders)
        {
            clip = Resources.Load<AudioClip>($"Voice/{folder}/{clipName}");
            if (clip != null)
            {
                Debug.Log($"Found in: Voice/{folder}/{clipName}");
                return clip;
            }
        }
        
        // Если не нашли через Resources.Load, ищем в файловой системе
        string voicePath = Path.Combine(Application.dataPath, "Resources", "Voice");
        
        if (Directory.Exists(voicePath))
        {
            string[] files = Directory.GetFiles(voicePath, $"{clipName}.*", SearchOption.AllDirectories);
            
            if (files.Length > 0)
            {
                // Конвертируем абсолютный путь в путь относительно Assets
                string relativePath = files[0].Replace(Application.dataPath, "Assets");
                clip = AssetDatabase.LoadAssetAtPath<AudioClip>(relativePath);
                
                if (clip != null)
                {
                    Debug.Log($"Found via file system: {relativePath}");
                    return clip;
                }
            }
        }
        
        return null;
    }
    
    void ClearDirectClips()
    {
        if (!EditorUtility.DisplayDialog(
            "Clear Direct Clips",
            "Удалить все Voice Clip Direct из этой главы?",
            "Yes", "No"))
        {
            return;
        }
        
        foreach (var dialogue in targetChapter.dialogues)
        {
            dialogue.voiceClipDirect = null;
        }
        
        EditorUtility.SetDirty(targetChapter);
        AssetDatabase.SaveAssets();
        
        Debug.Log("All direct clips cleared");
    }
}
#endif