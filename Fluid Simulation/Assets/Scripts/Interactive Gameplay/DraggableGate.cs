using Unity.VisualScripting;
using UnityEngine;

public class DraggableGate : Draggable
{
    [Header("Level 3 Gate Control Flags")]
    public bool isGate = false;
    public bool returnsToOriginalPosition = false;
    private Vector3 OriginalPosition;
    public float returningSmoothingSpeed = 0.005F;

    new void Start()
    {
        base.Start();
        OriginalPosition = transform.position;
    }

    new void Update()
    {
        base.Update();
        
        if(!isDragging && returnsToOriginalPosition){
            transform.position = Vector3.Lerp(transform.position, OriginalPosition, returningSmoothingSpeed);
        }
    }
    private new void HandleDragging()
    {
        if (isDragging)
        {
            if(isGate){
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3 dirtoMouse = mousePosition - transform.position;
                Vector3 targetPosition = transform.position;
                targetPosition.x += dirtoMouse.x * Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.z);
                targetPosition.y += dirtoMouse.y * Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.z);
                
                if (enableSmoothing)
                {
                    transform.position = Vector3.Lerp(transform.position, targetPosition, smoothingSpeed);
                }
                else
                {
                    transform.position = targetPosition;
                }
                
                
            }else{
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

    private void HandleResizing()
    {
        if (!resizable || !isDragging) return;

        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta == 0) return;

        // Calculate the scaling factor (10% per scroll tick)
        float scaleFactor = scrollDelta > 0 ? 1.1f : 0.9f;

        if (uniformScaling)
        {
            float newScaleX = targetScale.x * scaleFactor;
            float newScaleY = targetScale.y * scaleFactor;

            newScaleX = Mathf.Clamp(newScaleX, minScale, maxScale);
            newScaleY = Mathf.Clamp(newScaleY, minScale, maxScale);
            targetScale = new Vector3(newScaleX, newScaleY, targetScale.z);
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                // Horizontal scaling
                float newScaleX = targetScale.x * scaleFactor;
                newScaleX = Mathf.Clamp(newScaleX, minScale, maxScale);
                targetScale = new Vector3(newScaleX, targetScale.y, targetScale.z);
            }
            else
            {
                // Vertical scaling
                float newScaleY = targetScale.y * scaleFactor;
                newScaleY = Mathf.Clamp(newScaleY, minScale, maxScale);
                targetScale = new Vector3(targetScale.x, newScaleY, targetScale.z);
            }
        }
    }
}