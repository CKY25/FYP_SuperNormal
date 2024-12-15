using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyListUI : MonoBehaviour
{
    public static LobbyListUI Instance { get; private set; }


    [SerializeField] private Transform lobbySingleTemplate;
    [SerializeField] private Transform container;

    [SerializeField] private TMP_Text lobbyNameAssigned;
    [SerializeField] private TMP_Text lobbyCodeAssigned;
    [SerializeField] private TMP_Text gameModeAssigned;
    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container2;
    [SerializeField] private TMP_InputField lobbyCodeField;

    private string lobbyCode;

    private void Awake()
    {
        Instance = this;

        lobbySingleTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        if(!SceneManager.GetActiveScene().name.Equals("Level"))
        {
            if (container != null)
            {
                foreach (Transform child in container)
                {
                    if (child != null)
                    {
                        if (child == lobbySingleTemplate) continue;

                        DestroyImmediate(child.gameObject);
                    }
                }
            }

            foreach (Lobby lobby in lobbyList)
            {
                Transform lobbySingleTransform = Instantiate(lobbySingleTemplate, container);
                lobbySingleTransform.gameObject.SetActive(true);
                LobbyListSingleUI lobbyListSingleUI = lobbySingleTransform.GetComponent<LobbyListSingleUI>();
                lobbyListSingleUI.UpdateLobby(lobby);
            }
        }
    }

    public void setLobbyCode()
    {
        lobbyCode = lobbyCodeField.text;
    }

    public void joinPrivateLobby()
    {
        try
        {
            LobbyManager.Instance.JoinLobbyByCode(lobbyCode);
            Lobby joinedlobby = LobbyManager.Instance.getJoinedLobby();
            if (joinedlobby != null)
            {
                lobbyNameAssigned.text = joinedlobby.Name;
                lobbyCodeAssigned.text = joinedlobby.LobbyCode;
                gameModeAssigned.text = joinedlobby.Data["GameMode"].Value;
                LobbyUI.Instance.UpdateLobby();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void resetLobbyCode()
    {
        lobbyCodeField.GetComponent<TMP_InputField>().text = "";
    }
}
