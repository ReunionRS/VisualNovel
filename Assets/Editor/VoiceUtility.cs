#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Утилита для упрощения работы с озвучкой в визуальной новелле
/// </summary>
public class VoiceUtility : EditorWindow
{
    private ChapterData targetChapter;
    private string voiceFolderPath = "Assets/Audio/Voices";
    private bool autoAssignByIndex = true;
    private bool useDirectClips = true;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Visual Novel/Voice Utility", false, 40)]
    public static void ShowWindow()
    {
        VoiceUtility window = GetWindow<VoiceUtility>("Voice Utility");
        window.minSize = new Vector2(400, 500);
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Label("Voice Assignment Utility", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Эта утилита помогает быстро назначить озвучку для диалогов в главе.",
            MessageType.Info
        );
        
        EditorGUILayout.Space();
        
        // Выбор главы
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
        
        // Настройки
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        
        useDirectClips = EditorGUILayout.Toggle("Use Direct Audio Clips", useDirectClips);
        EditorGUILayout.HelpBox(
            useDirectClips 
                ? "Будет использоваться Voice Clip (Direct) - перетаскивание файлов" 
                : "Будет использоваться Voice Clip (Name) - имена из Resources/Voice/",
            MessageType.None
        );
        
        if (useDirectClips)
        {
            voiceFolderPath = EditorGUILayout.TextField("Voice Folder Path", voiceFolderPath);
            if (GUILayout.Button("Browse..."))
            {
                string path = EditorUtility.OpenFolderPanel("Select Voice Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.StartsWith(Application.dataPath))
                    {
                        voiceFolderPath = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
            }
        }
        
        autoAssignByIndex = EditorGUILayout.Toggle("Auto Assign By Index", autoAssignByIndex);
        EditorGUILayout.HelpBox(
            autoAssignByIndex
                ? "Автоматически назначит файлы по порядку (001.wav -> dialogue[0], 002.wav -> dialogue[1])"
                : "Позволит вручную выбрать файлы для каждого диалога",
            MessageType.None
        );
        
        EditorGUILayout.Space();
        
        // Информация о текущей главе
        GUILayout.Label("Chapter Info", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Chapter Title:", targetChapter.chapterTitle);
        EditorGUILayout.LabelField("Dialogue Count:", targetChapter.dialogues.Length.ToString());
        
        int voiceCount = 0;
        foreach (var dialogue in targetChapter.dialogues)
        {
            if (dialogue.voiceClipDirect != null || !string.IsNullOrEmpty(dialogue.voiceClipName))
                voiceCount++;
        }
        EditorGUILayout.LabelField("Dialogues with Voice:", $"{voiceCount} / {targetChapter.dialogues.Length}");
        
        EditorGUILayout.Space();
        
        // Кнопки действий
        GUILayout.Label("Actions", EditorStyles.boldLabel);
        
        if (useDirectClips && autoAssignByIndex)
        {
            if (GUILayout.Button("Auto Assign Voice Files", GUILayout.Height(30)))
            {
                AutoAssignVoiceFiles();
            }
        }
        
        if (GUILayout.Button("Clear All Voice Assignments", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog(
                "Clear Voice Assignments",
                "Удалить все назначения озвучки из этой главы?",
                "Yes", "No"))
            {
                ClearAllVoiceAssignments();
            }
        }
        
        if (GUILayout.Button("Enable Sync for All", GUILayout.Height(25)))
        {
            SetSyncForAll(true);
        }
        
        if (GUILayout.Button("Disable Sync for All", GUILayout.Height(25)))
        {
            SetSyncForAll(false);
        }
        
        EditorGUILayout.Space();
        
        // Список диалогов
        GUILayout.Label("Dialogues", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        for (int i = 0; i < targetChapter.dialogues.Length; i++)
        {
            DialogueEntry dialogue = targetChapter.dialogues[i];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"Dialogue {i}", EditorStyles.boldLabel);
            
            // Превью текста
            string preview = dialogue.text;
            if (preview.Length > 50)
                preview = preview.Substring(0, 50) + "...";
            EditorGUILayout.LabelField("Text:", preview);
            
            // Озвучка
            if (useDirectClips)
            {
                dialogue.voiceClipDirect = (AudioClip)EditorGUILayout.ObjectField(
                    "Voice Clip",
                    dialogue.voiceClipDirect,
                    typeof(AudioClip),
                    false
                );
            }
            else
            {
                dialogue.voiceClipName = EditorGUILayout.TextField("Voice Name", dialogue.voiceClipName);
            }
            
            dialogue.syncVoiceWithText = EditorGUILayout.Toggle("Sync", dialogue.syncVoiceWithText);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.EndScrollView();
        
        // Сохранение изменений
        if (GUI.changed)
        {
            EditorUtility.SetDirty(targetChapter);
        }
    }
    
    void AutoAssignVoiceFiles()
    {
        if (!Directory.Exists(voiceFolderPath))
        {
            EditorUtility.DisplayDialog("Error", $"Folder not found: {voiceFolderPath}", "OK");
            return;
        }
        
        // Получаем все аудио файлы
        string[] extensions = { "*.wav", "*.mp3", "*.ogg" };
        List<string> audioFiles = new List<string>();
        
        foreach (string ext in extensions)
        {
            string[] files = Directory.GetFiles(voiceFolderPath, ext);
            audioFiles.AddRange(files);
        }
        
        audioFiles.Sort();
        
        if (audioFiles.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "No audio files found in the selected folder", "OK");
            return;
        }
        
        int assigned = 0;
        
        for (int i = 0; i < targetChapter.dialogues.Length && i < audioFiles.Count; i++)
        {
            string assetPath = audioFiles[i];
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            
            if (clip != null)
            {
                targetChapter.dialogues[i].voiceClipDirect = clip;
                targetChapter.dialogues[i].syncVoiceWithText = true;
                assigned++;
            }
        }
        
        EditorUtility.SetDirty(targetChapter);
        AssetDatabase.SaveAssets();
        
        EditorUtility.DisplayDialog(
            "Success",
            $"Assigned {assigned} voice files to dialogues",
            "OK"
        );
    }
    
    void ClearAllVoiceAssignments()
    {
        foreach (var dialogue in targetChapter.dialogues)
        {
            dialogue.voiceClipDirect = null;
            dialogue.voiceClipName = "";
        }
        
        EditorUtility.SetDirty(targetChapter);
        AssetDatabase.SaveAssets();
        
        Debug.Log("All voice assignments cleared");
    }
    
    void SetSyncForAll(bool enabled)
    {
        foreach (var dialogue in targetChapter.dialogues)
        {
            dialogue.syncVoiceWithText = enabled;
        }
        
        EditorUtility.SetDirty(targetChapter);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Sync set to {enabled} for all dialogues");
    }
}
#endif