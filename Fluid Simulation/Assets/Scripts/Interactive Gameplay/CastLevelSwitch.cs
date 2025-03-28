using UnityEngine;
using DG.Tweening;
using System.Data;

public class CastLevelSwitch : MonoBehaviour
{
    [Header("Level References")]
    public ThermalSensor swordSensor;
    public BoundaryRestriction release;
    [Header("Lever Settings")]
    public bool isFlipped = false;

    [Header("Rotation Parameters")]
    public float rotationDuration = 0.1f; // Duration of rotation animation
    public float minAngle = 20;
    public float maxAngle = 160F;

    [Header("Optional References")]
    public AudioSource leverSound; // Optional audio feedback

    [Header("Interaction Settings and flags")]
    public bool canInteract = true; // Can the lever be interacted with?

    private Vector3 initialRotation;
    private Vector3 flippedRotation;
    private Tweener currentRotationTween;
    private bool pressed = false;
    void Start()
    {
        // Store initial rotation
        initialRotation = transform.localRotation.eulerAngles + new Vector3(0, 0, minAngle);

        // Calculate flipped rotation
        flippedRotation = transform.localRotation.eulerAngles + new Vector3(0, 0, maxAngle);

        // Set initial position based on isFlipped
        UpdateLeverPosition(false);
        if(swordSensor == null){
            Debug.LogError("Error: cast toggle not connected to thermal sensor");
        }
    }

    void FixedUpdate()
    {
        if(pressed){
            AnimateLeverRotation();
        }
        if(transform.eulerAngles.z > 90 & !isFlipped && swordSensor.metThreshold){
            release.maxY = 20F;
            isFlipped = true;
            PlayLeverEffects();
        }else if(transform.eulerAngles.z <= 90 & isFlipped){
            isFlipped = false;
            PlayLeverEffects();
        }        

    }

    /// <summary>
    /// check for player click on dial
    /// </summary>
    void OnMouseDown()
    {
        pressed = true;
        
    }

    /// <summary>
    /// checks if player release dial
    /// </summary>
	void OnMouseUp()
	{
		pressed = false;
        SetLeverRotation();
	}

    void AnimateLeverRotation()
    {
        // Kill any existing rotation tween
        currentRotationTween?.Kill();

        // Determine target rotation based on user input
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 toMouse = mousePos - transform.position;
        float angleToMouse = -1 * Mathf.Atan2(toMouse.x, toMouse.y) * Mathf.Rad2Deg;
        if(angleToMouse < minAngle){
            if(Mathf.Abs(angleToMouse) > 90){
                angleToMouse = maxAngle;
            }else{
                angleToMouse = minAngle;
            }
        }
        angleToMouse = Mathf.Clamp(angleToMouse, minAngle, maxAngle);

        // Vector3 targetRotation = isFlipped ? flippedRotation : initialRotation;
        Vector3 targetRotation = new Vector3(0, 0, angleToMouse);

        // Animate rotation using DOTween
        currentRotationTween = transform.DOLocalRotate(targetRotation, rotationDuration)
            .SetEase(Ease.InOutQuad) // Smooth rotation
            .OnComplete(OnLeverRotationComplete);
    }
    void SetLeverRotation()
    {
        // Kill any existing rotation tween
        currentRotationTween?.Kill();

        // Determine target rotation based on flipped state
        Vector3 targetRotation = isFlipped ? flippedRotation : initialRotation;

        // Animate rotation using DOTween
        currentRotationTween = transform.DOLocalRotate(targetRotation, rotationDuration)
            .SetEase(Ease.InOutQuad) // Smooth rotation
            .OnComplete(OnLeverRotationComplete);
    }

    void OnLeverRotationComplete()
    {
        // Optional: Additional logic when rotation is complete
        
        Debug.Log($"Lever flipped: {isFlipped}");
        
        
    }

    void PlayLeverEffects()
    {
        // Play sound if audio source is assigned
        if (leverSound != null)
        {
            leverSound.Play();
        }
    }

    void UpdateLeverPosition(bool animate = true)
    {
        // Immediately or animate to the current state
        if (animate)
        {
            SetLeverRotation();
        }
        else
        {
            // Directly set rotation without animation
            transform.localRotation = Quaternion.Euler(isFlipped ? flippedRotation : initialRotation);
        }
    }

    void OnDestroy()
    {
        // Ensure any active tweens are killed when object is destroyed
        currentRotationTween?.Kill();
    }

    // Optional method for external scripts to check lever state
    public bool IsLeverFlipped()
    {
        return isFlipped;
    }
}