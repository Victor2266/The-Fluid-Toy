using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrthographicCameraAdjuster : MonoBehaviour
{
    // The reference dimensions that your scene is designed for    
    // Based on a target orthographic size of 9.87 at 16:9:
    // Reference height = 2 × orthographic size = 2 × 9.87 = 19.74 units
    // Reference width = height × aspect ratio = 19.74 × (16/9) = 35.09 units

    public float referenceWidth = 35.09f;
    public float referenceHeight = 19.74f;
    private Camera cam;
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        if (!cam.orthographic)
        {
            Debug.LogError("This script requires an orthographic camera!");
            return;
        }
        
        AdjustCameraView();
    }
    
    private void Update()
    {
        // Only needed if you want to handle screen resizing at runtime
        AdjustCameraView();
    }
    
    private void AdjustCameraView()
    {
        // Calculate current aspect ratio
        float currentAspectRatio = (float)Screen.width / Screen.height;
        
        // Calculate reference aspect ratio
        float referenceAspectRatio = referenceWidth / referenceHeight;
        
        // Adjust orthographic size based on which dimension needs to be preserved
        if (currentAspectRatio >= referenceAspectRatio)
        {
            // Screen is wider than reference - maintain height
            cam.orthographicSize = referenceHeight / 2f;
        }
        else
        {
            // Screen is narrower than reference - maintain width
            float orthographicWidth = referenceWidth / 2f;
            cam.orthographicSize = orthographicWidth / currentAspectRatio;
        }
    }

    public void SetReferenceSizes(float orthographicSize) {
        referenceHeight = orthographicSize * 2f;
        referenceWidth = referenceHeight * (16f / 9f);
    }
}