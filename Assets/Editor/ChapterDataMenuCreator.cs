using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public class ChapterDataMenuCreator
{
    [MenuItem("Assets/Create/Visual Novel/Chapter Data", false, 1)]
    public static void CreateChapterData()
    {
        ChapterData newChapterData = ScriptableObject.CreateInstance<ChapterData>();
        
        newChapterData.chapterTitle = "Новая глава";
        newChapterData.chapterNumber = 1;
        
        newChapterData.dialogues = new DialogueEntry[0];
        
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets";
        }
        else if (System.IO.Path.GetExtension(path) != "")
        {
            path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }
        
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Chapter Data.asset");
        
        AssetDatabase.CreateAsset(newChapterData, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newChapterData;
        
        Debug.Log("Chapter Data created at: " + assetPathAndName);
    }
    
    [MenuItem("Tools/Visual Novel/Fix Chapter Data", false, 20)]
    public static void FixChapterData()
    {
        ChapterData selectedChapter = Selection.activeObject as ChapterData;
        
        if (selectedChapter == null)
        {
            Debug.LogError("Выберите Chapter Data для исправления");
            return;
        }
        
        if (selectedChapter.dialogues != null)
        {
            for (int i = 0; i < selectedChapter.dialogues.Length; i++)
            {
                if (selectedChapter.dialogues[i] != null)
                {
                    if (!string.IsNullOrEmpty(selectedChapter.dialogues[i].text))
                    {
                        string cleanText = selectedChapter.dialogues[i].text;
                        cleanText = cleanText.Replace("–", "-"); 
                        cleanText = cleanText.Replace("—", "-"); 
                        cleanText = cleanText.Replace("„", "\""); 
                        cleanText = cleanText.Replace("«", "\""); 
                        cleanText = cleanText.Replace("»", "\""); 
                        cleanText = cleanText.Replace("…", "..."); 
                        selectedChapter.dialogues[i].text = cleanText;
                    }
                    
                    if (selectedChapter.dialogues[i].backgroundMusic == null)
                    {
                        selectedChapter.dialogues[i].backgroundMusic = "";
                    }
                }
            }
        }
        
        EditorUtility.SetDirty(selectedChapter);
        AssetDatabase.SaveAssets();
        
        Debug.Log("Chapter Data исправлен: " + selectedChapter.name);
    }
    
    [MenuItem("Tools/Visual Novel/Add Music Field to All Chapters", false, 21)]
    public static void AddMusicFieldToAllChapters()
    {
        string[] guids = AssetDatabase.FindAssets("t:ChapterData");
        int fixedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ChapterData chapter = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
            
            if (chapter != null && chapter.dialogues != null)
            {
                bool needsUpdate = false;
                
                for (int i = 0; i < chapter.dialogues.Length; i++)
                {
                    if (chapter.dialogues[i] != null && chapter.dialogues[i].backgroundMusic == null)
                    {
                        chapter.dialogues[i].backgroundMusic = "";
                        needsUpdate = true;
                    }
                }
                
                if (needsUpdate)
                {
                    EditorUtility.SetDirty(chapter);
                    fixedCount++;
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Обновлено {fixedCount} Chapter Data файлов с полем backgroundMusic");
    }
    
    [MenuItem("Assets/Create/Visual Novel/Character Data", false, 2)]
    public static void CreateCharacterData()
    {
        Debug.Log("Character Data creation - coming soon!");
    }
}
#endif