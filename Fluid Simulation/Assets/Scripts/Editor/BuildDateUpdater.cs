using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;
using UnityEngine;

public class BuildDateUpdater : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // Get the path to the BuildDateText script
        string scriptPath = "Assets/Scripts/UI/BuildDateText.cs"; // Adjust this path to match your project structure

        if (!File.Exists(scriptPath))
        {
            Debug.LogError($"Script not found at path: {scriptPath}");
            return;
        }

        // Read the script file
        string[] lines = File.ReadAllLines(scriptPath);

        // Find and replace the BuildDate line with the current date
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains("public static string BuildDate"))
            {
                string formattedDate = $"public static string BuildDate = \"Build Date: {System.DateTime.Now:MMMM dd, yyyy}\";";
                lines[i] = formattedDate;
                break;
            }
        }

        // Write the updated lines back to the file
        File.WriteAllLines(scriptPath, lines);

        Debug.Log($"Updated BuildDate in {scriptPath} to {System.DateTime.Now:MMMM dd, yyyy}");
    }
}