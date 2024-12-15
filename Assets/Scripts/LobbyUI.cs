using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{

    public static LobbyUI Instance { get; private set; }

    [SerializeField] private Transform playerSingleTemplate;
    [SerializeField] private Transform container;
    [SerializeField] private TMP_Text lobbyNameAssigned;
    [SerializeField] private TMP_Text lobbyCodeAssigned;
    [SerializeField] private TMP_Text gameModeAssigned;
    [SerializeField] private Button startGameBtn;

    private void Awake()
    {
        Instance = this;

        playerSingleTemplate.gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateLobby();
    }

    private void UpdateLobby_Event(object sender, LobbyManager.LobbyEventArgs e)
    {
        UpdateLobby();
    }

    public void UpdateLobby()
    {
        if (LobbyManager.Instance.getJoinedLobby() != null)
            UpdateLobby(LobbyManager.Instance.getJoinedLobby());
    }

    private void UpdateLobby(Lobby lobby)
    {
        ClearLobby();

        lobbyNameAssigned.text = lobby.Name;
        lobbyCodeAssigned.text = lobby.LobbyCode;
        gameModeAssigned.text = lobby.Data["GameMode"].Value;

        foreach (Player player in lobby.Players)
        {
            Transform playerSingleTransform = Instantiate(playerSingleTemplate, container);
            playerSingleTransform.GetComponentInChildren<TMP_Text>().text = player.Data["PlayerName"].Value;
            playerSingleTransform.gameObject.SetActive(true);
            if(LobbyManager.Instance.IsLobbyHost())
            {
                startGameBtn.gameObject.SetActive(true);
            }
            else
            {
                startGameBtn.gameObject.SetActive(false);
            }
        }
    }

    public void ClearLobby()
    {
        foreach (Transform child in container)
        {
            if (child == playerSingleTemplate) continue;
            Destroy(child.gameObject);
        }
    }
}
