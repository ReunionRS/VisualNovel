#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ButtonFXAutoSetup
{
    [MenuItem("Tools/Visual Novel/Add Button Effects to All Buttons", false, 30)]
    public static void AddButtonEffectsToAllButtons()
    {
        Button[] allButtons = Object.FindObjectsOfType<Button>(true);
        int addedCount = 0;
        
        foreach (Button button in allButtons)
        {
            if (button.GetComponent<ButtonSoundFX>() == null)
            {
                ButtonSoundFX buttonFX = button.gameObject.AddComponent<ButtonSoundFX>();
                SetupButtonSounds(button, buttonFX);
                addedCount++;
                Debug.Log($"Added ButtonSoundFX to: {button.name}");
            }
        }
        
        Debug.Log($"Added button effects to {addedCount} buttons");
    }
    
    private static void SetupButtonSounds(Button button, ButtonSoundFX buttonFX)
    {
        string buttonName = button.name.ToLower();
        
        if (buttonName.Contains("confirm") || buttonName.Contains("ok") || buttonName.Contains("continue"))
        {
            buttonFX.SetSounds("button_hover", "button_confirm");
            buttonFX.SetHoverColor(new Color(0.8f, 1f, 0.8f, 1f));
        }
        else if (buttonName.Contains("cancel") || buttonName.Contains("back") || buttonName.Contains("exit"))
        {
            buttonFX.SetSounds("button_hover", "button_cancel");
            buttonFX.SetHoverColor(new Color(1f, 0.8f, 0.8f, 1f));
        }
        else if (buttonName.Contains("settings") || buttonName.Contains("options"))
        {
            buttonFX.SetSounds("button_hover", "button_settings");
            buttonFX.SetHoverColor(new Color(0.9f, 0.9f, 1f, 1f));
        }
        else if (buttonName.Contains("new") || buttonName.Contains("start"))
        {
            buttonFX.SetSounds("button_hover", "button_start");
            buttonFX.SetHoverColor(new Color(1f, 1f, 0.8f, 1f));
        }
        else
        {
            buttonFX.SetSounds("button_hover", "button_click");
        }
    }
    
    [MenuItem("Tools/Visual Novel/Remove All Button Effects", false, 31)]
    public static void RemoveAllButtonEffects()
    {
        ButtonSoundFX[] allButtonFX = Object.FindObjectsOfType<ButtonSoundFX>(true);
        int removedCount = 0;
        
        foreach (ButtonSoundFX buttonFX in allButtonFX)
        {
            Object.DestroyImmediate(buttonFX);
            removedCount++;
        }
        
        Debug.Log($"Removed button effects from {removedCount} buttons");
    }
    
    [MenuItem("Tools/Visual Novel/Setup UI Sound Manager", false, 32)]
    public static void SetupUISoundManager()
    {
        if (Object.FindObjectOfType<UISoundManager>() != null)
        {
            Debug.Log("UISoundManager already exists in scene");
            return;
        }
        
        GameObject uiSoundObj = new GameObject("UI Sound Manager");
        UISoundManager soundManager = uiSoundObj.AddComponent<UISoundManager>();
        
        Debug.Log("UISoundManager created and added to scene");
        Selection.activeGameObject = uiSoundObj;
    }
}
#endif