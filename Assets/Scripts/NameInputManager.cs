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
        // Настраиваем UI
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
            
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmName);
            
        // Настраиваем prompt текст
        if (promptText != null)
            promptText.text = "Как вас зовут?";
    }
    
    public void ShowNameInput()
    {
        Debug.Log("Showing name input");
        
        // Показываем панель ввода имени
        if (nameInputPanel != null)
        {
            nameInputPanel.SetActive(true);
            
            // Фокусируемся на поле ввода
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
        
        // Проверяем, что имя не пустое
        if (string.IsNullOrEmpty(playerName))
        {
            // Можно показать сообщение об ошибке
            Debug.LogWarning("Name cannot be empty");
            return;
        }
        
        // Ограничиваем длину имени
        if (playerName.Length > 20)
        {
            playerName = playerName.Substring(0, 20);
        }
        
        Debug.Log($"Player name set to: {playerName}");
        
        // Передаем имя в VisualNovelManager
        if (vnManager != null)
        {
            vnManager.SetPlayerName(playerName);
        }
        
        // Скрываем панель ввода
        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
            
        // Продолжаем диалог
        if (vnManager != null)
        {
            vnManager.ContinueAfterNameInput();
        }
    }
    
    // Обработка Enter в поле ввода
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