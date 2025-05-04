using System;
using TMPro;
using UnityEngine;

public class SandboxInitializer : MonoBehaviour
{
    private string[] sandboxSubtitles = {"4k", "8k", "16k", "32k", "64k", "128k", "256k", "512k", "1.024M", "2.048M",};
    public TMP_Text sandboxTitleText;
    private float[] cameraSizes = { 5.03f, 7.05f, 9.87f, 13.82f, 19.35f, 19.35f, 27.3f, 38.5f, 54.4f, 76.5f};
    public Camera sceneCamera;
    private int[] simulationMaxParticles = {4000, 8000, 16000, 32000, 64000, 128000, 256000, 512000, 1024000, 2048000};
    public IFluidSimulation sim;


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
        Destroy(gameObject);
    }
}
