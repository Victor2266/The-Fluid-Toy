using System;
using TMPro;
using UnityEngine;

public class SandboxInitializer : MonoBehaviour
{
    private string[] sandboxSubtitles = {"4k", "8k", "16k", "32k", "64k"};
    public TMP_Text sandboxTitleText;
    private float[] cameraSizes = { 5.03f, 7.05f, 9.87f, 13.82f, 19.35f};
    public Camera sceneCamera;

    // private Vector2[] simulationBounds = {new Vector2(17.81f, 10f), new Vector2(24.93f, 14f), new Vector2(34.9f, 19.6f), new Vector2(48.86f, 27.44f), new Vector2(68.40f, 38.42f)};
    private int[] simulationMaxParticles = {4000, 8000, 16000, 32000, 64000};
    public IFluidSimulation sim;

    // private float[] topObstacleBoundaryPositions = {12.5f, 14.5f, 17.3f, 21.22f, 26.71f};
    // private float[] sideObstacleBoundaryPositions = {16.41f, 19.965f, 24.95f, 31.93f, 41.7f};

    public GameObject[] obstacleBoundarys; // Top, Bottom, Left, Right

    private int lastWidth;
    private int lastHeight;

    void Awake()
    {
        // Set up title text
        int presetIndex = PlayerPrefs.GetInt("SandboxPreset", 2);
        string subtitle = sandboxSubtitles[presetIndex];
        sandboxTitleText.text = $"<b>Sandbox Mode</b>\n{subtitle} Particles\n\n";

        // Find the fluid simulation in the scene
        GameObject simulationGameobject = GameObject.FindGameObjectWithTag("Simulation");
        sim = simulationGameobject.GetComponent<IFluidSimulation>();

        // Set up camera
        sceneCamera.orthographicSize = cameraSizes[presetIndex];

        // Set up Max Particles
        sim.setMaxParticles(simulationMaxParticles[presetIndex]);

        // Destroy the sandbox initializer on complete
        // Destroy(gameObject);
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
