using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;

public class CastLevelFallMovement : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallDistance = 20F;
    public float movementTime = 1F;

    [Header("Trigger Button")]
    public CastLevelTriggerButton button;

    private Tweener currentTween;

    private Vector3 fellPosition;
    private bool fallen = false;

    void Start()
    {
        if(button == null){
            Debug.LogError("Error: no falling trigger connected");
        }
        
    }

    void FixedUpdate()
    {
        if(button.fall && !fallen){
            fall();
            fallen = true;
        }
    }

    void fall(){
        // Kill any existing tween to prevent multiple animations
        currentTween?.Kill();
        
        fellPosition = transform.position;
        fellPosition.y += fallDistance;
        // Move to open position smoothly
        currentTween = transform.DOMove(fellPosition, movementTime)
            .SetEase(Ease.OutQuad) // Optional: choose an easing function
            .OnComplete(() => {
                // Optional: You can add any completion logic here
                currentTween = null;
            });
    }
}