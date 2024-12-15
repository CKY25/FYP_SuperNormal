using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomNetworkManager))]
public class CustomNetworkManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get reference to the CustomNetworkManager instance
        CustomNetworkManager manager = (CustomNetworkManager)target;

        // Draw default Inspector GUI (this will draw any built-in NetworkManager fields)
        DrawDefaultInspector();

        // Display Player Prefabs (handling the list of prefabs)
        GUILayout.Label("Player Prefabs", EditorStyles.boldLabel);
        for (int i = 0; i < manager.playerPrefabs.Count; i++)
        {
            // Display each prefab in the list
            manager.playerPrefabs[i] = (GameObject)EditorGUILayout.ObjectField("Prefab " + (i + 1), manager.playerPrefabs[i], typeof(GameObject), false);
        }

        // Add a button to add a new prefab to the list
        if (GUILayout.Button("Add New Player Prefab"))
        {
            manager.playerPrefabs.Add(null); // Add a new entry to the list
        }

        // Optionally, remove the last prefab in the list
        if (manager.playerPrefabs.Count > 0 && GUILayout.Button("Remove Last Prefab"))
        {
            manager.playerPrefabs.RemoveAt(manager.playerPrefabs.Count - 1); // Remove last prefab
        }

        // Draw the spawn points fields
        GUILayout.Label("Spawn Points", EditorStyles.boldLabel);
        for (int i = 0; i < manager.spawnPoints.Count; i++)
        {  
            // Display each spawn point in the list
            manager.spawnPoints[i] = EditorGUILayout.Vector3Field("Spawn Point " + (i + 1), manager.spawnPoints[i]);
        }  
        
        // Add a button to add a new spawn point to the list
        if (GUILayout.Button("Add New Spawn Point"))
        {
            manager.spawnPoints.Add(Vector3.zero);  // Add a new entry to the list 
        } 
        
        // Optionally, remove the last spawn point in the list
        if (manager.spawnPoints.Count > 0 && GUILayout.Button("Remove Last Spawn Point"))
        {
            manager.spawnPoints.RemoveAt(manager.spawnPoints.Count - 1);  // Remove last spawn point 
        }

        // Make sure the changes to the object are applied
        EditorUtility.SetDirty(manager);
    }
}
