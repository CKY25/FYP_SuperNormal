using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using UnityEngine;
using Unity.Services.Lobbies;
using WebSocketSharp;
using System;

public class LobbyListSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI gameModeText;   

    private Lobby lobby;
    public GameObject root;
    private Animator animator;

    public void UpdateLobby(Lobby lobby)
    {
        this.lobby = lobby;

        lobbyNameText.text = lobby.Name;
        playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        gameModeText.text = lobby.Data[LobbyManager.KEY_GAME_MODE].Value;
    }

    public void joinLobby()
    {
        LobbyManager.Instance.JoinLobby(lobby);
    }

    public void showPasswordCode()
    {
        animator = root.GetComponent<Animator>();
        animator.SetBool("JoinClicked", false);
        animator.SetBool("JoinLobbyClicked", true);
    }

    public void ShowPasswordCodeXR()
    {
        root.transform.GetChild(5).gameObject.SetActive(true);
        transform.parent.gameObject.SetActive(false);
    }
}
