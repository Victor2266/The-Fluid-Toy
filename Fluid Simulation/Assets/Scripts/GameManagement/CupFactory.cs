using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CupFactory : MonoBehaviour
{
    [System.Serializable]
    public class CupInstance
    {
        public DrinkLevelManager.CupSize size;
        public GameObject cupObject;
        public string uniqueID;
        public System.DateTime spawnTime;
        public Transform[] cupColliders;
    }

    [Header("Cloning Settings")]
    [SerializeField] private DrinkLevelManager.CupSize cupSize;
    [SerializeField] private GameObject cupPrefab; // Obj to clone
    [SerializeField] private Vector2 spawnPosition = Vector2.zero;
    [SerializeField] private int maxClones = 5;

    [Header("Sensor Settings")]
    public bool useSensorManager = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private List<CupInstance> activeClones = new List<CupInstance>();

    private IFluidSimulation sim;
    private SensorManager sensorManager; // If it exists

    public AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GameObject simObject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simObject.GetComponent<IFluidSimulation>();

        if (useSensorManager)
        {
            GameObject sManagerObject = GameObject.FindGameObjectWithTag("SensorManager");
            sensorManager = sManagerObject.GetComponent<SensorManager>();
        }
    }
    private string GenerateUniqueID()
    {
        return System.Guid.NewGuid().ToString();
    }

    private GameObject SpawnCupClone()
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

        // play sound effect
        audioSource.Play();

        // Generate and assign unique ID
        string id = GenerateUniqueID();
        newCup.name = $"CupClone_{id}";

        // Find & tag colliders
        var colliders = newCup.GetComponentsInChildren<Transform>()
            .Where(t => t.CompareTag("BoxCollider") || t.CompareTag("SolidThermalBox"))
            .ToArray();

        // Record the instance
        CupInstance instance = new CupInstance
        {
            size = cupSize,
            cupObject = newCup,
            uniqueID = id,
            spawnTime = System.DateTime.Now,
            cupColliders = colliders
        };
        this.activeClones.Add(instance);

        // Update collider and sensor lists
        AddCollidersToSimulation(colliders);
        if (useSensorManager)
        {
            sensorManager.scanForSensors(); // Likewise
        }

        if (showDebugLogs) Debug.Log($"Spawned new cup with ID: {id}. Total clones: {activeClones.Count}");

        return newCup;
    }

    // Spawn a new clone at a specific position
    public GameObject SpawnCupCloneAtPosition(Vector2 position)
    {
        GameObject cup = this.SpawnCupClone();
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
                // Remove colliders from sim
                RemoveCollidersFromSimulation(activeClones[i].cupColliders);

                // Delete obj
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
    private bool DeleteOldestClone()
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
        foreach (var instance in this.activeClones)
        {
            ids.Add(instance.uniqueID);
        }
        return ids;
    }

    // Get a clone by ID
    public GameObject GetCloneByID(string id)
    {
        foreach (var instance in this.activeClones)
        {
            if (instance.uniqueID == id)
            {
                return instance.cupObject;
            }
        }
        return null;
    }

    public List<CupInstance> GetCups()
    {
        return this.activeClones;
    }

    private void AddCollidersToSimulation(Transform[] colliders)
    {
        // Get current colliders from simulation
        var currentColliders = sim.GetCurrentColliders().ToList();

        // Add new colliders
        currentColliders.AddRange(colliders);

        // Update simulation
        sim.SetColliders(currentColliders.ToArray());
    }

    private void RemoveCollidersFromSimulation(Transform[] colliders)
    {
        // Get current colliders from simulation
        var currentColliders = sim.GetCurrentColliders().ToList();

        // Remove specific colliders
        currentColliders.RemoveAll(c => colliders.Contains(c));

        // Update simulation
        sim.SetColliders(currentColliders.ToArray());
    }
}