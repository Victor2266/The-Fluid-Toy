using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContextMenuSetup : MonoBehaviour
{
    // This is a helper script to create the context menu prefab
    // You would run this once to create the prefab, then attach the prefab to your EditableObject script
    
    void Start()
    {
        CreateContextMenuPrefab();
    }
    
    public GameObject CreateContextMenuPrefab()
    {
        // Create the main container
        GameObject contextMenu = new GameObject("ContextMenuPrefab");
        RectTransform rectTransform = contextMenu.AddComponent<RectTransform>();
        Image background = contextMenu.AddComponent<Image>();
        
        // Set properties
        rectTransform.sizeDelta = new Vector2(300, 400);
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        // Add panel content
        CreateCloseButton(contextMenu);
        CreateDeleteButton(contextMenu);
        CreateDuplicateButton(contextMenu);
        
        // Create transform fields
        CreateInputField(contextMenu, "XScaleInput", "X Scale:", Vector2.up * -120);
        CreateInputField(contextMenu, "YScaleInput", "Y Scale:", Vector2.up * -160);
        CreateInputField(contextMenu, "ZRotationInput", "Z Rotation:", Vector2.up * -200);
        
        // Create ThermalBox fields
        CreateInputField(contextMenu, "ConductivityInput", "Conductivity:", Vector2.up * -240);
        CreateInputField(contextMenu, "TemperatureInput", "Temperature:", Vector2.up * -280);
        
        // Create SourceObject fields
        CreateInputField(contextMenu, "VelocityXInput", "Velocity X:", Vector2.up * -240);
        CreateInputField(contextMenu, "VelocityYInput", "Velocity Y:", Vector2.up * -280);
        CreateInputField(contextMenu, "FluidTypeInput", "Fluid Type:", Vector2.up * -320);
        CreateInputField(contextMenu, "SpawnRateInput", "Spawn Rate:", Vector2.up * -360);
        
        return contextMenu;
    }
    
    void CreateCloseButton(GameObject parent)
    {
        GameObject closeButton = CreateButton(parent, "CloseButton", "X", new Vector2(130, 180), new Vector2(40, 40));
        closeButton.GetComponent<Image>().color = new Color(0.8f, 0.2f, 0.2f);
    }
    
    void CreateDeleteButton(GameObject parent)
    {
        CreateButton(parent, "DeleteButton", "Delete", Vector2.up * -40, new Vector2(200, 40));
    }
    
    void CreateDuplicateButton(GameObject parent)
    {
        CreateButton(parent, "DuplicateButton", "Duplicate", Vector2.up * -80, new Vector2(200, 40));
    }
    
    GameObject CreateButton(GameObject parent, string name, string text, Vector2 position, Vector2 size)
    {
        GameObject button = new GameObject(name);
        RectTransform rectTransform = button.AddComponent<RectTransform>();
        Image image = button.AddComponent<Image>();
        Button buttonComponent = button.AddComponent<Button>();
        
        button.transform.SetParent(parent.transform, false);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        
        textRectTransform.sizeDelta = size;
        textRectTransform.anchoredPosition = Vector2.zero;
        
        textComponent.text = text;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontSize = 18;
        
        // Set up button colors
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f);
        buttonComponent.colors = colors;
        
        return button;
    }
    
    GameObject CreateInputField(GameObject parent, string name, string labelText, Vector2 position)
    {
        GameObject container = new GameObject(name);
        container.transform.SetParent(parent.transform, false);
        
        RectTransform rectTransform = container.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(260, 30);
        rectTransform.anchoredPosition = position;
        
        // Create label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(container.transform, false);
        
        RectTransform labelRectTransform = label.AddComponent<RectTransform>();
        TextMeshProUGUI labelComponent = label.AddComponent<TextMeshProUGUI>();
        
        labelRectTransform.sizeDelta = new Vector2(100, 30);
        labelRectTransform.anchoredPosition = new Vector2(-80, 0);
        
        labelComponent.text = labelText;
        labelComponent.color = Color.white;
        labelComponent.alignment = TextAlignmentOptions.Left;
        labelComponent.fontSize = 16;
        
        // Create input field
        GameObject inputField = new GameObject("Field");
        inputField.transform.SetParent(container.transform, false);
        
        RectTransform inputRectTransform = inputField.AddComponent<RectTransform>();
        Image inputBackground = inputField.AddComponent<Image>();
        TMP_InputField inputComponent = inputField.AddComponent<TMP_InputField>();
        
        inputRectTransform.sizeDelta = new Vector2(140, 30);
        inputRectTransform.anchoredPosition = new Vector2(60, 0);
        
        inputBackground.color = new Color(0.1f, 0.1f, 0.1f);
        
        // Create text area for input field
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputField.transform, false);
        
        RectTransform textAreaRectTransform = textArea.AddComponent<RectTransform>();
        textAreaRectTransform.sizeDelta = new Vector2(130, 20);
        textAreaRectTransform.anchoredPosition = Vector2.zero;
        
        // Create text component for input
        GameObject textComponent = new GameObject("Text");
        textComponent.transform.SetParent(textArea.transform, false);
        
        RectTransform textRectTransform = textComponent.AddComponent<RectTransform>();
        TextMeshProUGUI text = textComponent.AddComponent<TextMeshProUGUI>();
        
        textRectTransform.sizeDelta = new Vector2(130, 20);
        textRectTransform.anchoredPosition = Vector2.zero;
        
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        text.fontSize = 14;
        
        // Link text component to input field
        inputComponent.textComponent = text;
        inputComponent.textViewport = textAreaRectTransform;
        
        return container;
    }
}