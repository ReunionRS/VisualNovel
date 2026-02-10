#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor для ChapterData - корректное отображение массива диалогов
/// </summary>
[CustomEditor(typeof(ChapterData))]
public class ChapterDataEditor : Editor
{
    private SerializedProperty chapterTitle;
    private SerializedProperty chapterNumber;
    private SerializedProperty dialogues;
    private SerializedProperty nextChapter;
    
    private void OnEnable()
    {
        chapterTitle = serializedObject.FindProperty("chapterTitle");
        chapterNumber = serializedObject.FindProperty("chapterNumber");
        dialogues = serializedObject.FindProperty("dialogues");
        nextChapter = serializedObject.FindProperty("nextChapter");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Chapter Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(chapterTitle);
        EditorGUILayout.PropertyField(chapterNumber);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Dialogue", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dialogues, new GUIContent("Dialogues"), true);
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Next Chapter", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(nextChapter);
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
