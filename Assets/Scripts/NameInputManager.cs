using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameInputManager : MonoBehaviour
{
    [Header("Name Input UI")]
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public Button confirmButton;
    public TextMeshProUGUI promptText;
    
    [Header("References")]
    public VisualNovelManager vnManager;
    
    private void Start()
    {
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
            
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmName);
            
        if (promptText != null)
            promptText.text = "Как вас зовут?";
    }
    
    public void ShowNameInput()
    {
        Debug.Log("Showing name input");
        
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            
            if (nameInputField != null)
            {
                nameInputField.text = "";
                nameInputField.Select();
                nameInputField.ActivateInputField();
            }
        }
    }
    
    public void ConfirmName()
    {
        string playerName = nameInputField.text.Trim();
        
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogWarning("Name cannot be empty");
            return;
        }
        
        if (playerName.Length > 20)
        {
            playerName = playerName.Substring(0, 20);
        }
        
        Debug.Log($"Player name set to: {playerName}");
        
        if (vnManager != null)
        {
            vnManager.SetPlayerName(playerName);
        }
        
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
            
        if (vnManager != null)
        {
            vnManager.ContinueAfterNameInput();
        }
    }
    
    private void Update()
    {
        if (nameInputPanel != null && nameInputPanel.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                ConfirmName();
            }
        }
    }
}