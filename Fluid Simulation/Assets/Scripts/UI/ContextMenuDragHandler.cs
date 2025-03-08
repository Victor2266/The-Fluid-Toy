using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ContextMenuDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform menuRectTransform;
    private Canvas canvas;
    private RectTransform canvasRectTransform;
    private Vector2 offset;
    private bool isDragging = false;

    void Start()
    {
        // Get reference to the parent menu's RectTransform
        menuRectTransform = transform.parent.GetComponent<RectTransform>();
        if (menuRectTransform == null)
        {
            Debug.LogError("ContextMenuDragHandler must be attached to a child of an object with a RectTransform component.");
            enabled = false;
            return;
        }

        // Get the canvas reference
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("ContextMenuDragHandler requires a Canvas component in the hierarchy.");
            enabled = false;
            return;
        }

        // Get the canvas RectTransform
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        
        // Visual indicator that this area is draggable
        // Add a subtle highlight effect on hover if this is an Image
        Image dragAreaImage = GetComponent<Image>();
        if (dragAreaImage != null)
        {
            // Add a hover effect script
            gameObject.AddComponent<DragAreaHoverEffect>();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Calculate the offset between pointer position and menu position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            menuRectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out offset);
        
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        // Convert screen position to local position within the canvas
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, 
            eventData.position, 
            eventData.pressEventCamera, 
            out localPointerPosition))
        {
            // Update menu position based on drag, accounting for the initial offset
            menuRectTransform.localPosition = localPointerPosition - offset;
            
            // Ensure menu stays on screen
            EnsureMenuOnScreen();
        }
    }

    private void EnsureMenuOnScreen()
    {
        // Get screen boundaries in canvas space
        Vector3[] canvasCorners = new Vector3[4];
        canvasRectTransform.GetWorldCorners(canvasCorners);
        
        Vector3[] menuCorners = new Vector3[4];
        menuRectTransform.GetWorldCorners(menuCorners);
        
        // Convert to screen space
        for (int i = 0; i < 4; i++)
        {
            canvasCorners[i] = RectTransformUtility.WorldToScreenPoint(null, canvasCorners[i]);
            menuCorners[i] = RectTransformUtility.WorldToScreenPoint(null, menuCorners[i]);
        }
        
        // Calculate screen bounds
        float minX = canvasCorners[0].x;
        float maxX = canvasCorners[2].x;
        float minY = canvasCorners[0].y;
        float maxY = canvasCorners[2].y;
        
        // Calculate menu bounds
        float menuMinX = menuCorners[0].x;
        float menuMaxX = menuCorners[2].x;
        float menuMinY = menuCorners[0].y;
        float menuMaxY = menuCorners[1].y;
        
        // Calculate menu size
        float menuWidth = menuMaxX - menuMinX;
        float menuHeight = menuMaxY - menuMinY;
        
        // Calculate adjustments needed
        Vector3 position = menuRectTransform.position;
        
        // Adjust for right edge
        if (menuMaxX > maxX)
        {
            position.x -= (menuMaxX - maxX);
        }
        
        // Adjust for left edge
        if (menuMinX < minX)
        {
            position.x += (minX - menuMinX);
        }
        
        // Adjust for top edge
        if (menuMaxY > maxY)
        {
            position.y -= (menuMaxY - maxY);
        }
        
        // Adjust for bottom edge
        if (menuMinY < minY)
        {
            position.y += (minY - menuMinY);
        }
        
        menuRectTransform.position = position;
    }
}

// Optional hover effect component
public class DragAreaHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Color originalColor;
    private Image image;
    
    void Start()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image != null)
        {
            // Brighten slightly to indicate draggable area
            image.color = new Color(
                Mathf.Min(originalColor.r + 0.1f, 1f),
                Mathf.Min(originalColor.g + 0.1f, 1f),
                Mathf.Min(originalColor.b + 0.1f, 1f),
                originalColor.a
            );
            
            // Change cursor to indicate draggable area
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null)
        {
            // Restore original color
            image.color = originalColor;
            
            // Reset cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}