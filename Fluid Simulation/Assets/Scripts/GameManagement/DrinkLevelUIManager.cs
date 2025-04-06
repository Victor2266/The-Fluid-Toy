using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DrinkLevelUIManager : MonoBehaviour
{
    [Header("References")]
    public Transform incomingOrdersContainer; // Assign your "Incoming Orders" GameObject here
    public GameObject orderUIPrefab; // Assign your "Order" prefab here

    [Header("UI Settings")]
    public Vector2 orderElementSize = new Vector2(100, 100);
    public float spacing = 10f;
    public Color beerColor = new Color(0.95f, 0.65f, 0.21f); // Amber
    public Color alcoholColor = new Color(0.75f, 0.11f, 0.11f); // Red
    public Color colaColor = new Color(0.2f, 0.1f, 0f); // Brown
    public Color smallSizeColor = Color.white;
    public Color mediumSizeColor = Color.blue;
    public Color largeSizeColor = Color.red;

    private List<GameObject> activeOrderUIElements = new List<GameObject>();

    public void CreateOrderUI(FluidType drinkType, DrinkLevelManager.CupSize size, float timeLimit)
    {
        // Instantiate new order UI element
        GameObject newOrder = Instantiate(orderUIPrefab, incomingOrdersContainer);
        newOrder.name = $"Order_{drinkType}_{size}";
        activeOrderUIElements.Add(newOrder);

        // Set size and position
        RectTransform rt = newOrder.GetComponent<RectTransform>();
        rt.sizeDelta = orderElementSize;
        PositionOrderElements();

        // Set up components
        Image bgImage = newOrder.GetComponent<Image>();
        Text orderText = newOrder.GetComponentInChildren<Text>();
        OrderTimerUI timerUI = newOrder.GetComponent<OrderTimerUI>();

        // Customize appearance based on order
        switch (drinkType)
        {
            case FluidType.Beer:
                bgImage.color = beerColor;
                break;
            case FluidType.Alcohol:
                bgImage.color = alcoholColor;
                break;
            case FluidType.Cola:
                bgImage.color = colaColor;
                break;
        }

        // Add size indicator
        GameObject sizeIndicator = new GameObject("SizeIndicator");
        sizeIndicator.transform.SetParent(newOrder.transform);
        Image sizeImage = sizeIndicator.AddComponent<Image>();
        sizeImage.rectTransform.sizeDelta = new Vector2(20, 20);
        sizeImage.rectTransform.anchoredPosition = new Vector2(-orderElementSize.x / 2 + 15, orderElementSize.y / 2 - 15);

        switch (size)
        {
            case DrinkLevelManager.CupSize.Small:
                sizeImage.color = smallSizeColor;
                break;
            case DrinkLevelManager.CupSize.Medium:
                sizeImage.color = mediumSizeColor;
                break;
            case DrinkLevelManager.CupSize.Large:
                sizeImage.color = largeSizeColor;
                break;
        }

        // Set order text
        orderText.text = $"{drinkType}\n{size}";
        orderText.alignment = TextAnchor.MiddleCenter;
        orderText.color = Color.white;

        // Initialize timer if component exists
        if (timerUI != null)
        {
            timerUI.Initialize(timeLimit);
        }
    }

    public void RemoveOrderUI(string orderID)
    {
        GameObject orderToRemove = activeOrderUIElements.Find(o => o.name == orderID);
        if (orderToRemove != null)
        {
            activeOrderUIElements.Remove(orderToRemove);
            Destroy(orderToRemove);
            PositionOrderElements();
        }
    }

    private void PositionOrderElements()
    {
        for (int i = 0; i < activeOrderUIElements.Count; i++)
        {
            RectTransform rt = activeOrderUIElements[i].GetComponent<RectTransform>();
            float xPos = i * (orderElementSize.x + spacing);
            rt.anchoredPosition = new Vector2(xPos, 0);
        }
    }

    public void ClearAllOrders()
    {
        foreach (GameObject order in activeOrderUIElements)
        {
            Destroy(order);
        }
        activeOrderUIElements.Clear();
    }
}