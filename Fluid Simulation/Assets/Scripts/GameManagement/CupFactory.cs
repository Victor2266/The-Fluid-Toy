using System.Collections.Generic;
using UnityEngine;

public class CupFactory : MonoBehaviour
{
    [System.Serializable]
    public class CupInstance
    {
        public GameObject cupObject;
        public string uniqueID;
        public System.DateTime spawnTime;
    }

    [Header("Cloning Settings")]
    [SerializeField] private GameObject cupPrefab; // Obj to clone
    [SerializeField] private Vector2 spawnPosition = Vector2.zero;
    [SerializeField] private int maxClones = 5;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private List<CupInstance> activeClones = new List<CupInstance>();

    private string GenerateUniqueID()
    {
        return System.Guid.NewGuid().ToString();
    }

    public GameObject SpawnCupClone()
    {
        if (activeClones.Count >= maxClones)
        {
            if (showDebugLogs) Debug.LogWarning("Max clones reached. Cannot spawn more cups.");
            return null;
        }

        if (cupPrefab == null)
        {
            Debug.LogError("Cup prefab is not assigned!");
            return null;
        }

        // Instantiate the new cup
        GameObject newCup = Instantiate(cupPrefab, spawnPosition, Quaternion.identity);

        // Generate and assign unique ID
        string id = GenerateUniqueID();
        newCup.name = $"CupClone_{id}";

        // Record the instance
        CupInstance instance = new CupInstance
        {
            cupObject = newCup,
            uniqueID = id,
            spawnTime = System.DateTime.Now
        };

        activeClones.Add(instance);

        if (showDebugLogs) Debug.Log($"Spawned new cup with ID: {id}. Total clones: {activeClones.Count}");

        return newCup;
    }

    // Spawn a new clone at a specific position
    public GameObject SpawnCupCloneAtPosition(Vector2 position)
    {
        GameObject cup = SpawnCupClone();
        if (cup != null)
        {
            cup.transform.position = position;
        }
        return cup;
    }

    // Delete a specific clone by its ID
    public bool DeleteCloneByID(string id)
    {
        for (int i = 0; i < activeClones.Count; i++)
        {
            if (activeClones[i].uniqueID == id)
            {
                Destroy(activeClones[i].cupObject);
                activeClones.RemoveAt(i);

                if (showDebugLogs) Debug.Log($"Deleted cup with ID: {id}. Remaining clones: {activeClones.Count}");
                return true;
            }
        }

        if (showDebugLogs) Debug.LogWarning($"No cup found with ID: {id}");
        return false;
    }

    // Delete the oldest clone
    public bool DeleteOldestClone()
    {
        if (activeClones.Count == 0) return false;

        CupInstance oldest = activeClones[0];
        foreach (var instance in activeClones)
        {
            if (instance.spawnTime < oldest.spawnTime)
            {
                oldest = instance;
            }
        }

        return DeleteCloneByID(oldest.uniqueID);
    }

    // Delete all clones
    public void DeleteAllClones()
    {
        for (int i = activeClones.Count - 1; i >= 0; i--)
        {
            Destroy(activeClones[i].cupObject);
        }

        activeClones.Clear();

        if (showDebugLogs) Debug.Log("Deleted all cup clones");
    }

    // Get current clone count
    public int GetCloneCount()
    {
        return activeClones.Count;
    }

    // Get maximum allowed clones
    public int GetMaxClones()
    {
        return maxClones;
    }

    // Set maximum allowed clones (with cleanup if needed)
    public void SetMaxClones(int newMax)
    {
        if (newMax < 1) newMax = 1;

        maxClones = newMax;

        // If we're over the new limit, remove the oldest ones
        while (activeClones.Count > maxClones)
        {
            DeleteOldestClone();
        }
    }

    // Get all active clone IDs
    public List<string> GetAllCloneIDs()
    {
        List<string> ids = new List<string>();
        foreach (var instance in activeClones)
        {
            ids.Add(instance.uniqueID);
        }
        return ids;
    }

    // Get a clone by ID
    public GameObject GetCloneByID(string id)
    {
        foreach (var instance in activeClones)
        {
            if (instance.uniqueID == id)
            {
                return instance.cupObject;
            }
        }
        return null;
    }
}