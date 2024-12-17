using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    public RectTransform leftArrow; // The UI arrow on the left
    public RectTransform rightArrow; // The UI arrow on the right
    public CanvasGroup leftArrowCanvasGroup; // CanvasGroup for left arrow to control transparency
    public CanvasGroup rightArrowCanvasGroup; // CanvasGroup for right arrow to control transparency
    public float edgeThreshold = 50f; // Distance from the screen edge to trigger the arrow
    public float slideSpeed = 5f; // Speed of the arrow sliding in/out
    public float fadeSpeed = 2f; // Speed of the fade-in/out effect
    public float arrowHideOffset = 50f; // How far the arrow should slide off-screen when hidden
    public float arrowShowOffset = 50f; // How far the arrow should slide off-screen when visible

    private Vector3 leftArrowHiddenPosition;
    private Vector3 leftArrowVisiblePosition;
    private Vector3 rightArrowHiddenPosition;
    private Vector3 rightArrowVisiblePosition;

    void Start()
    {
        // Initialize the off-screen and visible positions for both arrows
        leftArrowHiddenPosition = new Vector3(-arrowHideOffset, leftArrow.anchoredPosition.y, 0f);
        leftArrowVisiblePosition = new Vector3(arrowShowOffset, leftArrow.anchoredPosition.y, 0f);
        rightArrowHiddenPosition = new Vector3(arrowHideOffset, rightArrow.anchoredPosition.y, 0f);
        rightArrowVisiblePosition = new Vector3(-arrowShowOffset, rightArrow.anchoredPosition.y, 0f);

        // Start arrows off-screen and fully transparent
        leftArrow.anchoredPosition = leftArrowHiddenPosition;
        rightArrow.anchoredPosition = rightArrowHiddenPosition;
        leftArrowCanvasGroup.alpha = 0f; // Hidden at start
        rightArrowCanvasGroup.alpha = 0f; // Hidden at start
    }

    void Update()
    {
        HandleArrowMovement();
    }

    private void HandleArrowMovement()
    {
        Vector2 mousePos = Input.mousePosition;

        // Handle left arrow slide and fade
        if (mousePos.x <= edgeThreshold) // Mouse near left edge
        {
            // Slide in the left arrow
            leftArrow.anchoredPosition = Vector3.Lerp(leftArrow.anchoredPosition, leftArrowVisiblePosition, Time.deltaTime * slideSpeed);
            // Fade in the left arrow
            leftArrowCanvasGroup.alpha = Mathf.Lerp(leftArrowCanvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
        }
        else
        {
            // Slide out the left arrow
            leftArrow.anchoredPosition = Vector3.Lerp(leftArrow.anchoredPosition, leftArrowHiddenPosition, Time.deltaTime * slideSpeed);
            // Fade out the left arrow
            leftArrowCanvasGroup.alpha = Mathf.Lerp(leftArrowCanvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
        }

        // Handle right arrow slide and fade
        if (mousePos.x >= Screen.width - edgeThreshold) // Mouse near right edge
        {
            // Slide in the right arrow
            rightArrow.anchoredPosition = Vector3.Lerp(rightArrow.anchoredPosition, rightArrowVisiblePosition, Time.deltaTime * slideSpeed);
            // Fade in the right arrow
            rightArrowCanvasGroup.alpha = Mathf.Lerp(rightArrowCanvasGroup.alpha, 1f, Time.deltaTime * fadeSpeed);
        }
        else
        {
            // Slide out the right arrow
            rightArrow.anchoredPosition = Vector3.Lerp(rightArrow.anchoredPosition, rightArrowHiddenPosition, Time.deltaTime * slideSpeed);
            // Fade out the right arrow
            rightArrowCanvasGroup.alpha = Mathf.Lerp(rightArrowCanvasGroup.alpha, 0f, Time.deltaTime * fadeSpeed);
        }
    }
}
