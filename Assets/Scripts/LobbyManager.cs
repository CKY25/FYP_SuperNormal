using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI PlayerNameTMP;
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private float refreshLobbyListTimer = 2f;
    private string playerName;

    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text errMsg;
    [SerializeField] private GameObject EnterNamePanel;
    public GameObject avatarObj;
    public Texture[] avatars;
    private Animator animator;
    private int avatarNo;

    public static LobbyManager Instance { get; private set; }
    public const string KEY_PLAYER_NAME = "PlayerName";
    public const string KEY_GAME_MODE = "GameMode";
    public const string KEY_START_GAME = "KEY_START_GAME";
    private string zero = "0";

    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class LobbyEventArgs : EventArgs
    {
        public Lobby lobby;
    }
    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    public enum GameMode
    {
        _1v1_,
        _2v2_
    }

    private async void Awake()
    {
        Instance = this;
        animator = root.GetComponent<Animator>();
        if (AuthenticationService.Instance.IsSignedIn)
            EnterNamePanel.SetActive(false);
        else
            EnterNamePanel.SetActive(true);

        if (AuthenticationService.Instance != null)
        {
            PlayerNameTMP.text = AuthenticationService.Instance.Profile;
        }

        if (PlayerPrefs.HasKey("AvatarNo"))
        {
            avatarNo = PlayerPrefs.GetInt("AvatarNo");
        }
        else
        {
            avatarNo = UnityEngine.Random.Range(0, avatars.Length);
            PlayerPrefs.SetInt("AvatarNo", avatarNo);
            PlayerPrefs.Save();
        }
        avatarObj.GetComponent<RawImage>().texture = avatars[avatarNo];
    }

    public async void Authenticate(string name)
    {
        //playerName = "Player" + UnityEngine.Random.Range(0, 99);
        playerName = name;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(playerName);
        await UnityServices.InitializeAsync(initializationOptions);

        AuthenticationService.Instance.SignedIn += () =>
        {
            //Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId + "\t" + AuthenticationService.Instance.Profile);
            RefreshLobbyList();
        };

        if(!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        PlayerNameTMP.text = playerName;

        if (PlayerPrefs.HasKey("AvatarNo"))
        {
            avatarNo = PlayerPrefs.GetInt("AvatarNo");
        }
        else
        {
            avatarNo = UnityEngine.Random.Range(0, avatars.Length);
            PlayerPrefs.SetInt("AvatarNo", avatarNo);
            PlayerPrefs.Save();
        }

        avatarObj.GetComponent<RawImage>().texture = avatars[avatarNo];
    }

    private void Update()
    {
        HandleRefreshLobbyList();
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdates();
    }

    private async void HandleLobbyHeartbeat()
    {
        if(IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if(heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15;
                heartbeatTimer = heartbeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    public async void RefreshLobbyList()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter> {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: zero)
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder> {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbyListQueryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = lobbyListQueryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void HandleRefreshLobbyList()
    {
            if (UnityServices.State == ServicesInitializationState.Initialized && AuthenticationService.Instance.IsSignedIn)
            {
                refreshLobbyListTimer -= Time.deltaTime;
                if (refreshLobbyListTimer < 0f)
                {
                    float refreshLobbyListTimerMax = 2f;
                    refreshLobbyListTimer = refreshLobbyListTimerMax;

                    RefreshLobbyList();
                }
            }
    }

    private async void HandleLobbyPollForUpdates()
    {
        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                float lobbyUpdateTimerMax = 1.1f;
                lobbyUpdateTimer = lobbyUpdateTimerMax;

                try
                {
                    Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = lobby;
                    if (joinedLobby.Data[KEY_START_GAME].Value != zero)
                    {
                        if (!IsLobbyHost())
                        {
                            RelayManager.Instance.JoinRelay(joinedLobby.Data[KEY_START_GAME].Value);
                        }
                    }
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogError($"Failed to get lobby: {e.Reason}");
                }
            }
        }
    }

    public async void CreatePrivateLobby(string lobbyName, int maxPlayers, GameMode gameMode)
    {
        try
        {
            Player player = GetPlayer();

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = player,
                IsPrivate = true,
                Data = new Dictionary<string, DataObject> {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) },
                { KEY_PLAYER_NAME, new DataObject(DataObject.VisibilityOptions.Public, AuthenticationService.Instance.Profile) },
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, zero) }
            }
            };

            Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;
            if (!SceneManager.GetActiveScene().name.Contains("VR"))
            {
                animator.SetBool("CreateClicked", false);
                animator.SetBool("LobbyCreated", true);
            }
            else
            {
                root.transform.GetChild(6).gameObject.SetActive(true);
                root.transform.GetChild(3).gameObject.SetActive(false);
            }
            LobbyUI.Instance.UpdateLobby();
        } catch (LobbyServiceException e)
        {
            showError(e, "CreateClicked");
        }
    }

    public async void CreatePublicLobby(string lobbyName, int maxPlayers, GameMode gameMode)
    {
        try
        {
            Player player = GetPlayer();

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                Player = player,
                Data = new Dictionary<string, DataObject> {
                { KEY_GAME_MODE, new DataObject(DataObject.VisibilityOptions.Public, gameMode.ToString()) },
                { KEY_PLAYER_NAME, new DataObject(DataObject.VisibilityOptions.Public, AuthenticationService.Instance.Profile) },
                { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, "0") }
            }
            };

            Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;
            joinedLobby = hostLobby;
            if (!SceneManager.GetActiveScene().name.Contains("VR"))
            {
                animator.SetBool("CreateClicked", false);
                animator.SetBool("LobbyCreated", true);
            }
            else
            {
                root.transform.GetChild(6).gameObject.SetActive(true);
                root.transform.GetChild(3).gameObject.SetActive(false);
            }
            LobbyUI.Instance.UpdateLobby();
        }
        catch (LobbyServiceException e)
        {
            showError(e, "CreateClicked");
        }
        catch (Exception e)
        {
            showException(e, "CreateClicked");
        }

    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            joinedLobby = lobby;
            if (!SceneManager.GetActiveScene().name.Contains("VR"))
            {
                animator.SetBool("JoinLobbyClicked", false);
                animator.SetBool("LobbyCreated", true);
            }
            else
            {
                root.transform.GetChild(6).gameObject.SetActive(true);
                root.transform.GetChild(5).gameObject.SetActive(false);
            }
            
        } catch (LobbyServiceException e)
        {
            showError(e, "JoinLobbyClicked");
        }
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby2 = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, joinLobbyByIdOptions);
            this.joinedLobby = lobby2;
            if (!SceneManager.GetActiveScene().name.Contains("VR"))
            {
                animator.SetBool("JoinClicked", false);
                animator.SetBool("LobbyCreated", true);
            }
            else
            {
                root.transform.GetChild(6).gameObject.SetActive(true);
                root.transform.GetChild(4).gameObject.SetActive(false);
            }
            LobbyUI.Instance.UpdateLobby();
        }
        catch (LobbyServiceException e)
        {
            showError(e, "JoinClicked");
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();

        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    public Player GetPlayer()
    {
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject> {
            { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationService.Instance.Profile) },
        });
    }

    public void PrintPlayers(Unity.Services.Lobbies.Models.Lobby lobby)
    {
        Debug.Log("Players in Lobby " + lobby.Name + " " + lobby.Data["GameMode"].Value);
        foreach(Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }

    private async void UpdateLobbyGameMode(string gameMode)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, gameMode) }
            }
            });

            joinedLobby = hostLobby;

            PrintPlayers(hostLobby);
        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void UpdatePlayerName(string newPlayerName)
    {
        try
        {
            playerName = newPlayerName;
            await LobbyService.Instance.UpdatePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
            {
                { KEY_PLAYER_NAME, new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName) }
            }
            });
        } catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
        
    }

    public async void LeaveLobby()
    {
        try
        {
            if(IsLobbyHost())
            {
                MigrateLobbyHost();
            }
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.ToString());
        }
    }

    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void MigrateLobbyHost()
    {
        try
        {
            if (joinedLobby.Players.Count > 1)
            {
                hostLobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    HostId = joinedLobby.Players[1].Id
                });

                joinedLobby = hostLobby;
                LobbyUI.Instance.UpdateLobby();
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void DeleteLobby()
    {
        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(LobbyManager.Instance.getHostLobby().Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }

    }

    public Lobby getJoinedLobby()
    {
        return this.joinedLobby;
    }

    public Lobby getHostLobby()
    {
        return this.hostLobby;
    }

    private void showError(LobbyServiceException e, string animationsBool)
    {
        errMsg.text = e.Reason.ToString();
        if (!SceneManager.GetActiveScene().name.Contains("VR"))
        {
            animator.SetBool(animationsBool, false);
            animator.SetBool("InvalidCode", true);
        }
        else
        {
            root.transform.GetChild(7).gameObject.SetActive(true);
        }
    }

    private void showException(Exception e, string animationsBool)
    {
        errMsg.text = e.Message.ToString();
        animator.SetBool(animationsBool, false);
        animator.SetBool("InvalidCode", true);
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void StartGame()
    {
        if(IsLobbyHost())
        {
            try
            {
                string relayCode = await RelayManager.Instance.CreateRelay(joinedLobby.MaxPlayers);
                Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { KEY_START_GAME, new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                joinedLobby = lobby;
            } catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}
