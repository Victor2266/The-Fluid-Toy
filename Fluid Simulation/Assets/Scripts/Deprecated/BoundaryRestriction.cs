using UnityEngine;

public class BoundaryRestriction : MonoBehaviour
{
    // Define the boundary limits
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    private Vector3 startingPosition;
    void Start()
    {
        startingPosition = transform.position;
    }
    void LateUpdate()
    {
        // Get the current position of the game object
        Vector3 position = transform.position;

        // Clamp the position within the specified boundaries
        position.x = Mathf.Clamp(position.x, startingPosition.x + minX, startingPosition.x + maxX);
        position.y = Mathf.Clamp(position.y, startingPosition.y + minY, startingPosition.y + maxY);

        // Update the position of the game object
        transform.position = position;
    }

    public void setStartingPosition(Vector3 pos)
    {
        startingPosition = pos;
    }
}
