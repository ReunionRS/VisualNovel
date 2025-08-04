using UnityEngine;
using UnityEditor;

public class ChapterDataMenuCreator
{
    [MenuItem("Assets/Create/Visual Novel/Chapter Data", false, 1)]
    public static void CreateChapterData()
    {
        ChapterData newChapterData = ScriptableObject.CreateInstance<ChapterData>();
        
        newChapterData.chapterTitle = "Новая глава";
        newChapterData.chapterNumber = 1;
        
        newChapterData.dialogues = new DialogueEntry[1];
        newChapterData.dialogues[0] = new DialogueEntry
        {
            text = "Введите текст диалога здесь...",
            backgroundImage = "",
            soundEffect = "",
            speakerName = "[ИМЯ ИГРОКА]"
        };
        
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
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
        
        Debug.Log($"Chapter Data created at: {assetPathAndName}");
    }
    
    // Дополнительный пункт меню для создания персонажа (на будущее)
    [MenuItem("Assets/Create/Visual Novel/Character Data", false, 2)]
    public static void CreateCharacterData()
    {
        Debug.Log("Character Data creation - coming soon!");
        // Здесь будет код для создания данных персонажа
    }
}