using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGameObjects : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public float spawnInterval = 1f;

    private void Start()
    {
        InvokeRepeating("Spawn", spawnInterval, spawnInterval);
    }

    private void Spawn()
    {
        StartCoroutine(SpawnAsync());
    }

    private IEnumerator SpawnAsync()
    {
        yield return new WaitForFixedUpdate();
        Instantiate(prefabToSpawn, transform.position, transform.rotation);
    }
}
