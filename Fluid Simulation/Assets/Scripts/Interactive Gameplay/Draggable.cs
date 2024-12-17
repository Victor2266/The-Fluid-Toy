using UnityEngine;

//Makes GameObject draggable
public class Draggable : MonoBehaviour
{   
    private bool isDragging = false;
    private Vector3 offset;

    public float smoothingSpeed = 0.2f; // Adjust this value to control the smoothing speed
    public bool enableSmoothing = true; // Boolean to enable/disable smoothing

    void OnMouseDown()
    {
        isDragging = true;
        offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPosition = mousePosition + offset;

            if (enableSmoothing)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothingSpeed);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }
}
