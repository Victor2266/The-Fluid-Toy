using Unity.VisualScripting;
using UnityEngine;

public class DraggableGate : Draggable
{
    [Header("Level 3 Gate Control Flags")]
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
}