using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderTextFollower : MonoBehaviour
{
    public RectTransform sliderRect;
    public TMP_Text textTMP;
    public float topPadding = 5f;

    private Image sliderImage;
    private RectTransform textRect;
    private float sliderHeight;

    void Start()
    {
        if (sliderRect == null || textTMP == null)
        {
            Debug.LogError("Slider Rect Transform or Text TMP not assigned!");
            enabled = false;
            return;
        }

        sliderImage = sliderRect.GetComponent<Image>();
        if (sliderImage == null || sliderImage.type != Image.Type.Filled)
        {
            Debug.LogError("Slider must have a Filled Image component!");
            enabled = false;
            return;
        }

        textRect = textTMP.GetComponent<RectTransform>();
        if (textRect == null)
        {
            Debug.LogError("Text TMP must have a Rect Transform component!");
            enabled = false;
            return;
        }

        sliderHeight = sliderRect.rect.height;
    }

    void Update()
    {
        if (sliderImage == null || textRect == null) return;

        float fillAmount = sliderImage.fillAmount;
        float sliderWidth = sliderRect.rect.width;

        float filledHeight = sliderHeight * fillAmount;
        float textY = sliderRect.localPosition.y + filledHeight - textRect.rect.height / 2 - topPadding;
        float textX = sliderRect.position.x; // Center horizontally

        textRect.localPosition = new Vector3(0f, textY, textRect.localPosition.z);
    }
}


// Separate script to set the text value (example)
public class SetSliderText : MonoBehaviour
{
    public TMP_Text textToSet;

    public void UpdateText(float sliderValue)
    {
        if (textToSet != null)
        {
            textToSet.text = sliderValue.ToString("F1"); // Example: Displaying the slider value
        }
    }
}