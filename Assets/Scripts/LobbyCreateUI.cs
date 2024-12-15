using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class LobbyCreateUI : MonoBehaviour
{
    public static LobbyCreateUI Instance { get; private set; }


    [SerializeField] private GameObject toggleSwitch;
    [SerializeField] private GameObject lobbynamefield;
    [SerializeField] private Toggle privateToggle;
    private TMP_Text lobbyNameInputField;
    private bool isPrivate = false;

    [SerializeField] private TMP_Text lobbyNameAssigned;
    [SerializeField] private TMP_Text lobbyCodeAssigned;
    [SerializeField] private TMP_Text gameModeAssigned;

    private string lobbyName;
    private int maxPlayers;
    private LobbyManager.GameMode gameMode;
    Lobby hostlobby;

    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;


    private void Update()
    {
        if(toggleSwitch.GetComponent<ToggleSwitch>().isOn)
        {
            gameMode = LobbyManager.GameMode._1v1_;
            maxPlayers = 2;
        }
        else
        {
            gameMode = LobbyManager.GameMode._2v2_;
            maxPlayers = 4;
        }
    }

    public void createLobby()
    {
        try
        {
            if (isPrivate)
            {
                LobbyManager.Instance.CreatePrivateLobby(lobbyName, maxPlayers, gameMode);
            }
            else
            {
                LobbyManager.Instance.CreatePublicLobby(lobbyName, maxPlayers, gameMode);
            }
        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    public void setLobbyName()
    {
        lobbyNameInputField = lobbynamefield.GetComponent<TMP_Text>();
        this.lobbyName = lobbyNameInputField.text;
    }

    public void setPrivate()
    {
        isPrivate = !isPrivate;
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    private void UpdateLobby()
    {
        UpdateLobby(LobbyManager.Instance.getJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby)
    {
        LobbyUI.Instance.ClearLobby();

        foreach (Player player in lobby.Players)
        {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.GetComponentInChildren<TMP_Text>().text = player.Data["PlayerName"].Value;
            playerSingleTransform.gameObject.SetActive(true);
        }
    }

    public void resetCreateGame()
    {
        toggleSwitch.GetComponent<ToggleSwitch>().resetToggle();
        lobbynamefield.gameObject.transform.parent.gameObject.transform.parent.GetComponent<TMP_InputField>().text = "";
        privateToggle.isOn = false;
        isPrivate = false;
    }
}
