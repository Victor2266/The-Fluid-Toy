using UnityEngine;

public class BoundaryRestriction : MonoBehaviour
{
    // Define the boundary limits
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    void Update()
    {
        // Get the current position of the game object
        Vector3 position = transform.position;

        // Clamp the position within the specified boundaries
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);

        // Update the position of the game object
        transform.position = position;
    }
}
