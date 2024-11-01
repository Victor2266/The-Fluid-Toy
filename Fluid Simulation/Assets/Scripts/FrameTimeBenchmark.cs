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
    private float elapsedTime = 0f;
    private bool isBenchmarking = false;
    
    // Statistics
    private float averageFrameTime = 0f;
    private float minFrameTime = float.MaxValue;
    private float maxFrameTime = float.MinValue;
    private float percentile90 = 0f;
    private float percentile95 = 0f;
    private float percentile99 = 0f;

    private void Update()
    {
        // Check for benchmark key press
        if (Input.GetKeyDown(startBenchmarkKey) && !isBenchmarking)
        {
            StartBenchmark();
        }

        if (!isBenchmarking) return;

        // Calculate frame time in milliseconds
        float frameTime = Time.deltaTime * 1000f;
        frameTimesSample.Add(frameTime);
        
        // Update min/max
        minFrameTime = Mathf.Min(minFrameTime, frameTime);
        maxFrameTime = Mathf.Max(maxFrameTime, frameTime);
        
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
        elapsedTime = 0f;
        minFrameTime = float.MaxValue;
        maxFrameTime = float.MinValue;
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

        // Calculate average
        averageFrameTime = frameTimesSample.Average();

        // Sort for percentiles
        var sortedFrameTimes = frameTimesSample.OrderBy(x => x).ToList();
        int count = sortedFrameTimes.Count;

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
                        $"\nAverage Frame Time: {averageFrameTime:F2}ms ({1000f/averageFrameTime:F1} FPS)" +
                        $"\nMin Frame Time: {minFrameTime:F2}ms ({1000f/minFrameTime:F1} FPS)" +
                        $"\nMax Frame Time: {maxFrameTime:F2}ms ({1000f/maxFrameTime:F1} FPS)";

        if (logDetailedStats)
        {
            results += $"\n90th Percentile: {percentile90:F2}ms" +
                      $"\n95th Percentile: {percentile95:F2}ms" +
                      $"\n99th Percentile: {percentile99:F2}ms";
        }

        Debug.Log(results);
    }

    private void OnGUI()
    {
        // Always show the key instruction
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label($"Press {startBenchmarkKey} to start benchmark");

        if (visualizeInEditor && isBenchmarking)
        {
            GUILayout.Label($"Benchmark Progress: {(elapsedTime/benchmarkDuration*100):F1}%");
            GUILayout.Label($"Current Frame Time: {(Time.deltaTime * 1000f):F2}ms");
            GUILayout.Label($"Current FPS: {(1.0f/Time.deltaTime):F1}");
        }
        GUILayout.EndArea();
    }
}