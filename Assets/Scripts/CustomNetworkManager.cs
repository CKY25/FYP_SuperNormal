using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance { get; private set; }

    [SerializeField]
    public List<GameObject> playerPrefabs;
    [SerializeField]
    public List<Vector3> spawnPoints = new List<Vector3>();
    private List<int> usedSpawnIndices = new List<int>();

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    public void OnClientConnected(ulong clientId)
    {
        SpawnPlayerServerRpc(clientId);
    }

    [ServerRpc]
    private void SpawnPlayerServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            {
                GameObject randomPrefab = playerPrefabs[Random.Range(0, playerPrefabs.Count)];
                Vector3 spawnPosition = GetRandomSpawnPoint();
                GameObject playerObject = Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
                playerObject.transform.parent = null;
                playerObject.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        if (usedSpawnIndices.Count == spawnPoints.Count) 
        { 
            Debug.LogWarning("All spawn points have been used. Resetting used spawn points."); 
            usedSpawnIndices.Clear(); 
        }

        int randomIndex; 
        do 
        { 
            randomIndex = Random.Range(0, spawnPoints.Count); 
        } while (usedSpawnIndices.Contains(randomIndex)); 
        usedSpawnIndices.Add(randomIndex); 

        return spawnPoints[randomIndex];
    }
}
