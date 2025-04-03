using UnityEngine;

public class CupButton : MonoBehaviour
{
    [SerializeField] CupFactory factory;
    [SerializeField] Vector2 spawnPoint;

    void OnMouseDown()
    {
        factory.SpawnCupCloneAtPosition(spawnPoint);
    }
}