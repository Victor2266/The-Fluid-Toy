using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
public class CastLevelCastMovement : MonoBehaviour
{
    [Header("Button Reference")]
    public CastLevelCastButton button;

    [Header("Opening/Closing Settings")]
    public float distance = 5F;
    public float movementTime = 1f;

    private Vector3 startingPosition;

    private Vector3 openPosition;

    private Tweener currentTween;
    void Start()
    {
        if(button == null){
            Debug.LogError("Error: button not connected to cast script");
        }
        startingPosition = transform.position;
        openPosition = transform.position;
        openPosition.x += distance;
    }

    void FixedUpdate()
    {
        if(button.isOpening){
            if(transform.position.x != openPosition.x && currentTween == null){
                moveToOpenPosition();
            }
        }else if(button.isClosing){
            if(transform.position.x != startingPosition.x && currentTween == null){
                moveToClosePosition();
            }
        }
    }

    void moveToOpenPosition()
    {
        // Kill any existing tween to prevent multiple animations
        currentTween?.Kill();

        // Move to open position smoothly
        currentTween = transform.DOMove(openPosition, movementTime)
            .SetEase(Ease.OutQuad) // Optional: choose an easing function
            .OnComplete(() => {
                // Optional: You can add any completion logic here
                button.isOpened = true;
                button.isOpening = false;
                currentTween = null;
            });
    }

    void moveToClosePosition()
    {
        // Kill any existing tween to prevent multiple animations
        currentTween?.Kill();

        // Move to open position smoothly
        currentTween = transform.DOMove(startingPosition, movementTime)
            .SetEase(Ease.OutQuad) // Optional: choose an easing function
            .OnComplete(() => {
                // Optional: You can add any completion logic here
                button.isOpened = false;
                button.isClosing = false;
                currentTween = null;
            });
    }

}