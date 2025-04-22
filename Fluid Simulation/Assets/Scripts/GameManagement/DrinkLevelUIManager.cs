using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrinkLevelUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject orderBoxPrefab; // Sprite-based prefab with SpriteRenderer
    public Transform orderZone; // Reference to the Order Zone transform
    public float horizontalSpacing = 0.5f; // Space between order boxes in world units

    [Header("Appearance Settings")]
    public Color normalTextColor = Color.black;
    public Color warningTextColor = Color.yellow;
    public Color criticalTextColor = Color.red;
    public Font textFont;
    public int fontSize = 14;
    public Color boxColor = new Color(1f, 1f, 1f, 0.8f); // Semi-transparent white

    private List<GameObject> activeOrderBoxes = new List<GameObject>();
    private Bounds zoneBounds;
    private float boxWidth;
    private Dictionary<string, TextMesh> orderTextMeshes = new Dictionary<string, TextMesh>();

    void Start()
    {
        if (orderZone == null)
        {
            Debug.LogError("Order Zone reference not set in DrinkLevelUIManager!");
            return;
        }

        // Calculate zone bounds
        zoneBounds = new Bounds(orderZone.position, orderZone.localScale);

        // Calculate box dimensions (1/6 of order zone width, full height)
        boxWidth = zoneBounds.size.x / 6f;
    }

    public void CreateOrderUI(FluidType drinkType, DrinkLevelManager.CupSize size, float timeLimit)
    {
        if (orderBoxPrefab == null)
        {
            Debug.LogError("Order Box Prefab not set!");
            return;
        }

        // Create new order box
        GameObject newOrderBox = Instantiate(orderBoxPrefab, orderZone);
        newOrderBox.name = $"Order_{drinkType}_{size}";

        // Position the box (world space)
        float xPos = zoneBounds.min.x + (boxWidth / 2f) + ((boxWidth + horizontalSpacing) * activeOrderBoxes.Count);
        float yPos = zoneBounds.center.y;
        newOrderBox.transform.position = new Vector3(xPos, yPos, orderZone.position.z);

        // Scale the box to full height and 1/6 width of zone
        Vector3 boxScale = new Vector3(
            boxWidth,
            zoneBounds.size.y * 0.9f, // 90% of height to leave some margin
            1f
        );
        newOrderBox.transform.localScale = boxScale;

        // Set box color
        SpriteRenderer boxRenderer = newOrderBox.GetComponent<SpriteRenderer>();
        if (boxRenderer != null)
        {
            boxRenderer.color = boxColor;
        }

        // Create text display
        GameObject textObj = new GameObject("OrderText");
        textObj.transform.SetParent(newOrderBox.transform);
        textObj.transform.localPosition = Vector3.zero;

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = $"{size}\n{drinkType}";
        if (!float.IsPositiveInfinity(timeLimit))
        {
            textMesh.text += $"\n00:{timeLimit:00}";
        }

        // Configure text appearance
        textMesh.characterSize = 0.1f;
        textMesh.fontSize = fontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = normalTextColor;
        textMesh.font = textFont;

        // Center the text in the box
        textObj.transform.localPosition = new Vector3(0, 0, -0.1f); // Slightly in front of box

        // Store reference to the text mesh for updates
        orderTextMeshes.Add(newOrderBox.name, textMesh);

        // Add to active list
        activeOrderBoxes.Add(newOrderBox);
    }

    public void RemoveOrderUI(string orderID)
    {
        // Find and remove the order box
        for (int i = 0; i < activeOrderBoxes.Count; i++)
        {
            if (activeOrderBoxes[i].name == orderID)
            {
                // Remove from dictionary first
                if (orderTextMeshes.ContainsKey(orderID))
                {
                    orderTextMeshes.Remove(orderID);
                }

                Destroy(activeOrderBoxes[i]);
                activeOrderBoxes.RemoveAt(i);
                RePositionOrderBoxes(); // Reorganize remaining orders
                break;
            }
        }
    }

    public void UpdateOrderTimer(string orderID, string formattedTime, Color timeColor)
    {
        if (orderTextMeshes.TryGetValue(orderID, out TextMesh textMesh))
        {
            // Extract the base order text (first two lines)
            string[] lines = textMesh.text.Split('\n');
            if (lines.Length >= 2)
            {
                // Convert color to hex for TextMesh (which doesn't support rich text)
                // For actual color changing, we'll modify the whole text color
                textMesh.text = $"{lines[0]}\n{lines[1]}\n{formattedTime}";
                textMesh.color = timeColor; // This changes all text color
            }
        }
    }

    private void RePositionOrderBoxes()
    {
        // Reposition all order boxes to account for removed ones
        for (int i = 0; i < activeOrderBoxes.Count; i++)
        {
            float xPos = zoneBounds.min.x + (boxWidth / 2f) + ((boxWidth + horizontalSpacing) * i);
            float yPos = zoneBounds.center.y;
            activeOrderBoxes[i].transform.position = new Vector3(xPos, yPos, orderZone.position.z);
        }
    }
}