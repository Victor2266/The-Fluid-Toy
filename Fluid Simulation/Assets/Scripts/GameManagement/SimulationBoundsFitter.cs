using UnityEngine;

public class SimulationBoundsFitter : MonoBehaviour
{
    private int lastWidth;
    private int lastHeight;
    public IFluidSimulation sim;
    public GameObject[] obstacleBoundarys; // Top, Bottom, Left, Right
    public Camera sceneCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Find the fluid simulation in the scene
        GameObject simulationGameobject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simulationGameobject.GetComponent<IFluidSimulation>();

        UpdateBoundsSizes();
    }

    private void UpdateBoundsSizes(){
        // Set up camera
        float boundsWidth = sceneCamera.orthographicSize * 2f * Screen.width / Screen.height;
        float boundsHeight = sceneCamera.orthographicSize * 2f;

        // Set up Obstacle Boundary colliders positions
        obstacleBoundarys[0].transform.localPosition = new Vector3(0, (boundsHeight + 15f)/2f, 0);
        obstacleBoundarys[1].transform.localPosition = new Vector3(0, -(boundsHeight + 15f)/2f, 0);
        obstacleBoundarys[2].transform.localPosition = new Vector3(-(boundsWidth + 15f)/2f, 0, 0);
        obstacleBoundarys[3].transform.localPosition = new Vector3((boundsWidth + 15f)/2f, 0, 0);

        // Set up Obstacle Boundary colliders sizes
        obstacleBoundarys[0].GetComponent<BoxCollider2D>().size = new Vector2((boundsHeight + 15f)/2f * 3.5f, 15f);
        obstacleBoundarys[1].GetComponent<BoxCollider2D>().size = new Vector2((boundsHeight + 15f)/2f * 3.5f, 15f);
        obstacleBoundarys[2].GetComponent<BoxCollider2D>().size = new Vector2(15f, (boundsWidth + 15f)/2f * 2f);
        obstacleBoundarys[3].GetComponent<BoxCollider2D>().size = new Vector2(15f, (boundsWidth + 15f)/2f * 2f);

        // Update simulation settings
        sim.setBounds(new Vector2(boundsWidth, boundsHeight));   
    }

    void Update()
    {
        // Check if resolution has changed
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            UpdateBoundsSizes();
            // Update the stored resolution
            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
    }
}
