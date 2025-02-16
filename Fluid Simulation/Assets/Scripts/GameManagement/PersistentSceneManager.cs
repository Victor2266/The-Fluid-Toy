using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentSceneManager : MonoBehaviour
{
    private static PersistentSceneManager instance;
    private bool isLoading = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static PersistentSceneManager Instance
    {
        get { return instance; }
    }

    public void LoadLevel(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("Attempted to load level while already loading");
            return;
        }
        isLoading = true;
        //StartCoroutine(SafelyLoadScene(sceneName));

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        isLoading = false;
    }

    private IEnumerator SafelyLoadScene(string sceneName)
    {
        //Debug.Log($"Starting to load scene: {sceneName}");

        // First find and clean up the simulation
        GameObject simulation = GameObject.FindGameObjectWithTag("Simulation");
        IFluidSimulation simulationScript = simulation.GetComponent<IFluidSimulation>();
        IParticleDisplay particleDisplay = simulation.GetComponent<IParticleDisplay>();
        if (simulation != null)
        {
            //Debug.Log("Found simulation, releasing compute buffers");
            
            // Explicitly release compute buffers first
            simulationScript.ReleaseComputeBuffers();
            particleDisplay.ReleaseBuffers();
            DestroyImmediate(simulation);

            //Debug.Log("Destroyed simulation object");
        }
        else
        {
            Debug.Log("No simulation object found to clean up");
        }

        // Wait two frames to ensure destruction is processed
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // Force immediate cleanup
        //Resources.UnloadUnusedAssets();
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        
        //Debug.Log("Starting to load new scene");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            //Debug.Log($"Loading progress: {asyncLoad.progress * 100}%");
            yield return null;
        }

        //Debug.Log("Scene loaded in background, preparing for activation");
        
        // One final cleanup before activation
        System.GC.WaitForPendingFinalizers();
        yield return new WaitForEndOfFrame();

        //Debug.Log("Activating new scene");
        asyncLoad.allowSceneActivation = true;

        // Wait for completion with timeout
        float timeoutTimer = 0f;
        float timeoutLimit = 30f;
        while (!asyncLoad.isDone)
        {
            timeoutTimer += Time.deltaTime;
            if (timeoutTimer > timeoutLimit)
            {
                Debug.LogError("Scene activation timed out!");
                break;
            }
            yield return null;
        }

        //Debug.Log("Scene load completed");
        isLoading = false;
    }

}