using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FrameTimeBenchmark : MonoBehaviour
{
    [Header("Benchmark Settings")]
    [SerializeField] private float benchmarkDuration = 10f;
    [SerializeField] private bool logDetailedStats = true;
    [SerializeField] private bool visualizeInEditor = true;
    [SerializeField] private KeyCode startBenchmarkKey = KeyCode.B;
    
    private List<float> frameTimesSample = new List<float>();
    private List<double> cpuFrameTimesSample = new List<double>();
    private List<double> gpuFrameTimesSample = new List<double>();
    private float elapsedTime = 0f;
    private bool isBenchmarking = false;

    // Frame timing data
    private UnityEngine.FrameTiming[] frameTimings = new UnityEngine.FrameTiming[2];
    
    // Statistics
    private float averageFrameTime = 0f;
    private double averageCpuFrameTime = 0f;
    private double averageGpuFrameTime = 0f;
    private float minFrameTime = float.MaxValue;
    private float maxFrameTime = float.MinValue;
    private double minCpuFrameTime = double.MaxValue;
    private double maxCpuFrameTime = double.MinValue;
    private double minGpuFrameTime = double.MaxValue;
    private double maxGpuFrameTime = double.MinValue;
    private float percentile90 = 0f;
    private float percentile95 = 0f;
    private float percentile99 = 0f;

    private void Start()
    {
        // Enable frame timing stats
        UnityEngine.FrameTimingManager.CaptureFrameTimings();
    }

    private void Update()
    {
        // Check for benchmark key press
        if (Input.GetKeyDown(startBenchmarkKey) && !isBenchmarking)
        {
            StartBenchmark();
        }

        if (!isBenchmarking) return;

        UnityEngine.FrameTimingManager.CaptureFrameTimings();
        uint numFrames = UnityEngine.FrameTimingManager.GetLatestTimings((uint)frameTimings.Length, frameTimings);

        if (numFrames > 0)
        {
            float frameTime = Time.deltaTime * 1000f;
            double cpuFrameTime = frameTimings[0].cpuFrameTime;
            double gpuFrameTime = frameTimings[0].gpuFrameTime;

            frameTimesSample.Add(frameTime);
            cpuFrameTimesSample.Add(cpuFrameTime);
            gpuFrameTimesSample.Add(gpuFrameTime);
            
            // Update min/max
            minFrameTime = Mathf.Min(minFrameTime, frameTime);
            maxFrameTime = Mathf.Max(maxFrameTime, frameTime);
            minCpuFrameTime = System.Math.Min(minCpuFrameTime, cpuFrameTime);
            maxCpuFrameTime = System.Math.Max(maxCpuFrameTime, cpuFrameTime);
            minGpuFrameTime = System.Math.Min(minGpuFrameTime, gpuFrameTime);
            maxGpuFrameTime = System.Math.Max(maxGpuFrameTime, gpuFrameTime);
        }
        
        elapsedTime += Time.deltaTime;
        
        if (elapsedTime >= benchmarkDuration)
        {
            CalculateStatistics();
            StopBenchmark();
        }
    }

    public void StartBenchmark()
    {
        if (isBenchmarking)
        {
            Debug.LogWarning("Benchmark already in progress!");
            return;
        }

        frameTimesSample.Clear();
        cpuFrameTimesSample.Clear();
        gpuFrameTimesSample.Clear();
        elapsedTime = 0f;
        minFrameTime = float.MaxValue;
        maxFrameTime = float.MinValue;
        minCpuFrameTime = double.MaxValue;
        maxCpuFrameTime = double.MinValue;
        minGpuFrameTime = double.MaxValue;
        maxGpuFrameTime = double.MinValue;
        isBenchmarking = true;
        Debug.Log($"Starting benchmark for {benchmarkDuration} seconds...");
    }

    private void StopBenchmark()
    {
        isBenchmarking = false;
        LogResults();
    }

    private void CalculateStatistics()
    {
        if (frameTimesSample.Count == 0) return;

        // Calculate averages
        averageFrameTime = frameTimesSample.Average();
        averageCpuFrameTime = cpuFrameTimesSample.Average();
        averageGpuFrameTime = gpuFrameTimesSample.Average();

        // Sort for percentiles
        var sortedFrameTimes = frameTimesSample.OrderBy(x => x).ToList();
        
        // Calculate percentiles
        percentile90 = CalculatePercentile(sortedFrameTimes, 0.9f);
        percentile95 = CalculatePercentile(sortedFrameTimes, 0.95f);
        percentile99 = CalculatePercentile(sortedFrameTimes, 0.99f);
    }

    private float CalculatePercentile(List<float> sortedData, float percentile)
    {
        int index = Mathf.RoundToInt(percentile * (sortedData.Count - 1));
        return sortedData[index];
    }

    private void LogResults()
    {
        string results = $"\nBenchmark Results ({frameTimesSample.Count} frames):" +
                        $"\n\nTotal Frame Time:" +
                        $"\n  Average: {averageFrameTime:F2}ms ({1000f/averageFrameTime:F1} FPS)" +
                        $"\n  Min: {minFrameTime:F2}ms ({1000f/minFrameTime:F1} FPS)" +
                        $"\n  Max: {maxFrameTime:F2}ms ({1000f/maxFrameTime:F1} FPS)" +
                        $"\n\nCPU Frame Time:" +
                        $"\n  Average: {averageCpuFrameTime:F2}ms" +
                        $"\n  Min: {minCpuFrameTime:F2}ms" +
                        $"\n  Max: {maxCpuFrameTime:F2}ms" +
                        $"\n\nGPU Frame Time:" +
                        $"\n  Average: {averageGpuFrameTime:F2}ms" +
                        $"\n  Min: {minGpuFrameTime:F2}ms" +
                        $"\n  Max: {maxGpuFrameTime:F2}ms" +
                        $"\n\nCpu/Gpu Ratio: {averageCpuFrameTime/averageGpuFrameTime:F2}x";

        if (logDetailedStats)
        {
            results += $"\n\nPercentiles (Total Frame Time):" +
                      $"\n  90th: {percentile90:F2}ms" +
                      $"\n  95th: {percentile95:F2}ms" +
                      $"\n  99th: {percentile99:F2}ms";
        }

        Debug.Log(results);
    }

    private void OnGUI()
    {
        // Always show the key instruction
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label($"Press {startBenchmarkKey} to start benchmark");

        if (visualizeInEditor && isBenchmarking)
        {
            GUILayout.Label($"Benchmark Progress: {(elapsedTime/benchmarkDuration*100):F1}%");
            GUILayout.Label($"Current Frame Time: {(Time.deltaTime * 1000f):F2}ms");
            if (frameTimings.Length > 0)
            {
                GUILayout.Label($"Current CPU Time: {frameTimings[0].cpuFrameTime:F2}ms");
                GUILayout.Label($"Current GPU Time: {frameTimings[0].gpuFrameTime:F2}ms");
            }
            GUILayout.Label($"Current FPS: {(1.0f/Time.deltaTime):F1}");
        }
        GUILayout.EndArea();
    }
}