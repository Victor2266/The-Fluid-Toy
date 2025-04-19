using UnityEngine;
using TMPro;

public class BuildDateText : MonoBehaviour
{
    // This will hold the build date as a string
public static string BuildDate = "Build Date: April 19, 2025";

    private void Start()
    {
        // Get the TextMeshPro component attached to this GameObject
        TextMeshProUGUI textMeshPro = GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        // Set the text to the build date
        if (textMeshPro != null)
        {
            textMeshPro.text = BuildDate;
        }
        else
        {
            Debug.LogError("No TextMeshPro component found on this GameObject.");
        }
    }
}
