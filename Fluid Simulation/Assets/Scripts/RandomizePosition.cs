using UnityEngine;

//Randomizes the position of gameobject of start of scene
public class RandomizePosition : MonoBehaviour
{
    // Define the range for randomization
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -10f;
    public float maxY = 10f;

    void Start()
    {
        // Generate random x and y positions within the specified range
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        // Set the new position of the game object
        transform.position = new Vector3(randomX, randomY, transform.position.z);
    }
}
