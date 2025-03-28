using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
public class CastLevelCastMovement : MonoBehaviour
{
    [Header("Button Reference")]
    public CastLevelSwitch castSwitch;

    [Header("Opening/Closing Settings")]
    public float distance = 5F;
    public float movementTime = 1f;

    private Vector3 startingPosition;

    private Vector3 openPosition;

    private Tweener currentTween;
    public bool fall = false;
    private bool isOpened = false;
    void Start()
    {
        if(castSwitch == null){
            Debug.LogError("Error: button not connected to cast script");
        }
        startingPosition = transform.position;
        openPosition = transform.position;
        openPosition.x += distance;
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

        // Move to open position smoothly
        currentTween = transform.DOMove(openPosition, movementTime)
            .SetEase(Ease.OutQuad) // Optional: choose an easing function
            .OnComplete(() => {
                // Optional: You can add any completion logic here
                isOpened = true;
                currentTween = null;
                fall = true;
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
                isOpened = false;
                currentTween = null;
            });
    }

}