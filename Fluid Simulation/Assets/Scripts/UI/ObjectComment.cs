using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectComment : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField] private string comment = "";
    
    [Tooltip("If true, the comment will show up in the scene view")]
    [SerializeField] private bool showInScene = true;
    
    [SerializeField] private Color commentColor = Color.yellow;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showInScene && !string.IsNullOrEmpty(comment))
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = commentColor;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            
            // Draw the comment above the object
            Vector3 position = transform.position;
            position.y += 0.5f; // Offset above the object
            
            Handles.Label(position, comment, style);
            
            // Draw a line connecting the text to the object
            Gizmos.color = commentColor;
            Gizmos.DrawLine(transform.position, position);
        }
    }

    // Custom editor to make it look nicer in the Inspector
    [CustomEditor(typeof(ObjectComment))]
    public class ObjectCommentEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ObjectComment commentComponent = (ObjectComment)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("GameObject Comment", EditorStyles.boldLabel);
            
            EditorGUILayout.HelpBox("This script is for adding comments to gameobjects.\nComments will be visible in both the Inspector and (optionally) the Scene view.", MessageType.Info);
            EditorGUILayout.Space();
            
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("comment"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showInScene"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("commentColor"));
            
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.Space();
        }
    }
#endif
}