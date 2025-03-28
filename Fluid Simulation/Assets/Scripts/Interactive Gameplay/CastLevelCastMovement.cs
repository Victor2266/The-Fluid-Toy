using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
public class CastLevelCastMovement : MonoBehaviour
{
    [Header("Button Reference")]
    public CastLevelSwitch castSwitch;
    public CastingLevelManager manager;

    [Header("Opening/Closing Settings")]
    public float distance = 5F;
    public float movementTime = 1f;
    public bool isOpened = false;

    private Vector3 startingPosition;

    private Vector3 openPosition;

    private Tweener currentTween;
    
    void Start()
    {
        if(castSwitch == null){
            Debug.LogError("Error: button not connected to cast script");
        }

    }

    void FixedUpdate()
    {
        if(castSwitch.isFlipped && !isOpened){
            if(transform.position.x != openPosition.x && currentTween == null){
                moveToOpenPosition();
            }
        }else if(!castSwitch.isFlipped && isOpened){
            if(transform.position.x != startingPosition.x && currentTween == null){
                moveToClosePosition();
            }
        }
    }

    void moveToOpenPosition()
    {
        // Kill any existing tween to prevent multiple animations
        currentTween?.Kill();
        openPosition = transform.position;
        openPosition.x += distance;

        // Move to open position smoothly
        currentTween = transform.DOMove(openPosition, movementTime)
            .SetEase(Ease.OutQuad) // Optional: choose an easing function
            .OnComplete(() => {
                // Optional: You can add any completion logic here
                isOpened = true;
                currentTween = null;
            });
    }

    void moveToClosePosition()
    {
        // Kill any existing tween to prevent multiple animations
        currentTween?.Kill();
        startingPosition = transform.position;
        // Move to open position smoothly
        currentTween = transform.DOMove(startingPosition, movementTime)
            .SetEase(Ease.OutQuad) // Optional: choose an easing function
            .OnComplete(() => {
                // Optional: You can add any completion logic here
                isOpened = false;
                currentTween = null;
            });
    }

}