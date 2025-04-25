using UnityEngine;

public class CupButton : MonoBehaviour
{
    [SerializeField] private CupFactory factory;
    [SerializeField] private Vector2 spawnPoint;

    void OnMouseDown()
    {
        factory.SpawnCupCloneAtPosition(spawnPoint);
    }
}