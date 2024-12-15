using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private string levelName = "Level";

    private void Awake()
    {
        Instance = this;
    }

    public async Task<string> CreateRelay(int numPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numPlayers-1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene(levelName, LoadSceneMode.Single);
            //NetworkManager.DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
            //NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;

            return joinCode;

        } catch(RelayServiceException e)
        {
            Debug.LogError(e.Reason);
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            //NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;

        } catch (RelayServiceException e)
        {
            Debug.LogError(e.Reason);
        }
    }

    private void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        // Both client and server receive these notifications
        switch (sceneEvent.SceneEventType)
        {
            // Handle client to server LoadComplete notifications
            case SceneEventType.LoadComplete:
                {
                    sceneEvent.AsyncOperation.allowSceneActivation = false;
                    break;
                }

            // Handle Server to Client Load Complete (all clients finished loading notification)
            case SceneEventType.LoadEventCompleted:
                {
                    // This will let you know when all clients have finished loading a scene
                    // Received on both server and clients
                    foreach (var sceneEvent2 in sceneEvent.ClientsThatCompleted)
                    {
                        sceneEvent.AsyncOperation.allowSceneActivation = true;
                    }
                    break;
                }
        }
    }
}
