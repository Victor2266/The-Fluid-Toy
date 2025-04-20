using UnityEngine;
using UnityEngine.UI;

public class OrderTimerUI : MonoBehaviour
{
    public Rect timerRect = new Rect(10, 10, 80, 20); // Position and size of timer
    private float maxTime;
    private float remainingTime;
    private bool isActive = true;

    public void Initialize(float timeLimit)
    {
        maxTime = timeLimit;
        remainingTime = timeLimit;
    }

    void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        else if (isActive)
        {
            isActive = false;
            // Handle timer expiration if needed
        }
    }

    void OnGUI()
    {
        if (!isActive) return;

        // Calculate fill percentage
        float fillPercent = remainingTime / maxTime;

        // Set colors based on time remaining
        Color timerColor;
        if (fillPercent > 0.6f)
            timerColor = Color.green;
        else if (fillPercent > 0.3f)
            timerColor = Color.yellow;
        else
            timerColor = Color.red;

        // Draw background box
        GUI.color = Color.grey;
        GUI.Box(timerRect, "");

        // Draw filled portion
        Rect fillRect = new Rect(timerRect);
        fillRect.width *= fillPercent;
        GUI.color = timerColor;
        GUI.Box(fillRect, "");

        // Draw time text
        GUI.color = Color.white;
        int seconds = Mathf.CeilToInt(remainingTime);
        string timeText = $"{seconds / 60:00}:{seconds % 60:00}";
        GUI.Label(timerRect, timeText, GetCenteredStyle());
    }

    private GUIStyle GetCenteredStyle()
    {
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        centeredStyle.normal.textColor = Color.white;
        return centeredStyle;
    }
}