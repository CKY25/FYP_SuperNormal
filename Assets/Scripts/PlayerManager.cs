using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class PlayerManager : NetworkBehaviour
{
    public TextMeshProUGUI Desc1;
    public TextMeshProUGUI Desc2;
    private Abilities[] abilities;
    public NetworkVariable<AbilityData> selfAbility = new NetworkVariable<AbilityData>();
    private string selectedAbilityName;
    private static NetworkVariable<int> team1Count = new NetworkVariable<int>(0);
    private static NetworkVariable<int> team2Count = new NetworkVariable<int>(0);
    private static NetworkVariable<StringSerializer> chosenAbility = new NetworkVariable<StringSerializer>(new StringSerializer());
    private static NetworkVariable<int> playerCount = new NetworkVariable<int>(0);
    private static NetworkVariable<int> playersReadyCount = new NetworkVariable<int>(0);
    private static NetworkVariable<bool> team1Disabled = new NetworkVariable<bool>(false);
    private static NetworkVariable<bool> team2Disabled = new NetworkVariable<bool>(false);
    private static NetworkVariable<bool> allLoaded = new NetworkVariable<bool>(false);
    private int team = 1;

    [SerializeField]
    public Vector3 mapMinBounds;
    [SerializeField]
    public Vector3 mapMaxBounds;

    private string superStrength = "SuperStrength";
    private string invisible = "Invisible";
    private string flight = "Flight";
    private string magnetism = "Magnetism";
    private string surveillance = "Surveillance";
    private string taser = "Taser";
    private string thermalGlass = "ThermalGlass";
    private string luope = "Luope";
    private string flashDesc = "Flash\r\nThe power to possess physical strength far beyond that of a normal human. To pretend you are an NPC, what you need to do is to move very very very slowly... as moving too fast will break items which increases your suspicious level.";
    private string invisibleDesc = "Invisibility\r\nYou will slowly become invisible if you are not moving for 10 seconds. To pretend you are an NPC, what you need to do is to move around more frequently or else you will be easily be detected as a superhuman due to your ability.";
    private string magnetismDesc = "Magnetism\r\nBe aware of your surroundings! Standing too close to a metal object will attract it towards you. To pretend you are an NPC, what you need to do is to ensure you are not too close to any metal objects around you.";
    private string flightDesc = "Flight\r\nYou will be in flying state every 10 seconds if you did not press the hold to ground button. To pretend you are an NPC, make sure you press the hold to ground button once in a while so you will not be flying uncontrollably.";
    private string cctvDesc = "Surveillance\r\nYou are able to place a small camera in any place and use it once in a while as another eye to watch an area.";
    private string taserDesc = "Taser\r\nYou are able to stun any character fo a short while using the taser gun. By stunning them, you can identify whether they are NPC or player. It can also ensure you eliminate a player without him running away.";
    private string thermalDesc = "Thermal Glass\r\nYou can equip this thermal glass anytime and it can identify the invisible player who has gone invisible. But take note that it will be useless if no one chooses invisible ability.";
    private string loupeDesc = "Loupe\r\nYou can use this tool once in a game by choosing any 1 of the characters and it will reveal whether that character is an NPC or a player. It is useful to double confirm whether the character is a player and to exclude known NPC.";
    private string SuperNormiesDesc = "SuperNormies\r\nYou will have a special power that you have to control carefully so you will not expose yourself.";
    private string NormiesDesc = "Normies\r\nYou will be equipped with a tool to help you to find out who is the SuperNormies.";

    private void Awake()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            abilities = GetComponent<AbilityHolder>().ability;
        }

        GetComponent<PlayerController>().enabled = false;
        GetComponent<MissionManager>().enabled = false;
        GetComponent<AbilityHolder>().enabled = false;
        GetComponent<EliminateSuperNormies>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<TimerDisplay>().enabled = false;
    }

    private void Start()
    {

        if (IsOwner)
        {
            //Player camera
            transform.GetChild(3).GetChild(0).gameObject.SetActive(true);

            transform.GetChild(4).GetChild(1).gameObject.SetActive(false);
            transform.GetChild(4).GetChild(2).gameObject.SetActive(false);
            transform.GetChild(4).GetChild(3).gameObject.SetActive(false);
            transform.GetChild(4).GetChild(4).gameObject.SetActive(false);
            transform.GetChild(4).GetChild(5).gameObject.SetActive(false);
            transform.GetChild(4).GetChild(6).gameObject.SetActive(false);
            transform.GetChild(4).GetChild(7).gameObject.SetActive(false);
            transform.GetChild(4).GetChild(8).gameObject.SetActive(false);
            transform.GetChild(4).GetChild(9).gameObject.SetActive(false);
            transform.GetChild(1).GetChild(8).gameObject.SetActive(true);

            if (isPresent())
            {
                transform.GetComponent<XROrigin>().enabled = true;
                transform.GetComponent<CharacterControllerDriver>().enabled = true;
                transform.GetComponent<XRInputModalityManager>().enabled = true;
                transform.GetComponent<PlayerInput>().enabled = true;
                transform.GetComponent<InputActionManagerXR>().enabled = true;

                transform.GetChild(3).GetChild(3).gameObject.SetActive(true);
                transform.GetChild(3).GetChild(4).gameObject.SetActive(true);
                transform.GetChild(3).GetChild(5).gameObject.SetActive(true);
                transform.GetChild(3).GetChild(6).gameObject.SetActive(true);
                transform.GetChild(8).gameObject.SetActive(true);
                transform.GetChild(9).gameObject.SetActive(true);
                transform.GetChild(10).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(0).GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                transform.GetChild(1).GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            }

            if(!NetworkManager.Singleton.IsServer && isPresent())
                transform.GetChild(8).GetComponent<InputActionManager>().enabled = true;
        }
    }

    private void OnPlayerValueChange(int oldValue, int newValue)
    {
        if (playerCount.Value == LobbyManager.Instance.getJoinedLobby().Players.Count)
        {
            setAllLoadedServerRpc(true);
        }

    }

    private void OnAllLoadedValueChange(bool oldValue, bool newValue)
    {
        if (IsOwner)
        {
            StartCoroutine(LoadingScreen());
        }
    }

    private IEnumerator LoadingScreen()
    {
        yield return new WaitForSeconds(1);
        transform.GetChild(1).GetChild(8).gameObject.SetActive(false);

        if (string.IsNullOrEmpty(selectedAbilityName))
            transform.GetChild(1).GetChild(6).gameObject.SetActive(true);
    }

    public void SetTeam(int team)
    {
        if (IsOwner)
        {
            if (team == 1)
            {
                transform.GetChild(1).GetChild(6).GetChild(1).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                transform.GetChild(1).GetChild(6).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(6).GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = SuperNormiesDesc;
                this.team = 1;
            }
            else if (team == 2)
            {
                transform.GetChild(1).GetChild(6).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                transform.GetChild(1).GetChild(6).GetChild(1).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(6).GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = NormiesDesc;
                this.team = 2;
            }
            
        }
    }

    public void SelectTeam()
    {
        if (IsOwner)
        {
            transform.GetChild(1).GetChild(6).gameObject.SetActive(false);
            if (team == 1)
            {
                incTeam1ServerRpc();
                transform.GetChild(1).GetChild(4).gameObject.SetActive(true);

            }
            else if (team == 2)
            {
                incTeam2ServerRpc();
                transform.GetChild(1).GetChild(5).gameObject.SetActive(true);
            }
        }
    }

    private void OnTeam1ValueChange(int oldValue, int newValue)
    {
        if (IsServer)
        {
            if (team1Count.Value == NetworkManager.Singleton.ConnectedClients.Count / 2)
            {
                DisableRoleButtonServerRpc(1);
                team1Disabled.Value = true;
            }
        }  
    }

    private void OnTeam2ValueChange(int oldValue, int newValue)
    {
        if (IsServer)
        {
            if (team2Count.Value == NetworkManager.Singleton.ConnectedClients.Count / 2)
            {
                DisableRoleButtonServerRpc(2);
                team2Disabled.Value = true;
            }
        }
    }

    private void OnTeam1DisabledChange(bool oldValue, bool newValue)
    {
        if (IsOwner && team1Disabled.Value)
        {
            team = 2;
            transform.GetChild(1).GetChild(6).GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = NormiesDesc;
        }
    }

    private void OnTeam2DisabledChange(bool oldValue, bool newValue)
    {
        if (IsOwner && team2Disabled.Value)
        {
            team = 1;
            transform.GetChild(1).GetChild(6).GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = SuperNormiesDesc;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void DisableRoleButtonServerRpc(int team)
    {
        DisableRoleButtonClientRpc(team);
    }

    [ClientRpc]
    private void DisableRoleButtonClientRpc(int team)
    {
        gameObject.transform.GetChild(1).GetChild(6).GetChild(team).GetComponent<Button>().interactable = false;
        gameObject.transform.GetChild(1).GetChild(6).GetChild(team).GetComponent<MainMenuBtn>().enabled = false;
        gameObject.transform.GetChild(1).GetChild(6).GetChild(team).GetChild(0).GetComponent<Image>().color = Color.grey;
    }

    private void OnPlayerReadyCountChange(int oldValue, int newValue)
    {
        if (playersReadyCount.Value == LobbyManager.Instance.getJoinedLobby().Players.Count)
        {
            StartCoroutine(Wait());
        }
    }

    private void OnChosenAbilityChange(StringSerializer oldValue, StringSerializer newValue)
    {
        if (IsOwner)
        {
            if (chosenAbility.Value != null && selectedAbilityName != null)
            {
                for (int i = 0; i < chosenAbility.Value.chosenAbilityEnum.Length; i++)
                {
                    if (chosenAbility.Value.chosenAbilityEnum[i] == '1')
                    {
                        DisableButtonServerRpc(superStrength);
                        if (selectedAbilityName != null && selectedAbilityName.Equals(superStrength))
                        {
                            selectedAbilityName = null;
                        }
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '2')
                    {
                        DisableButtonServerRpc(invisible);
                        if (selectedAbilityName != null && selectedAbilityName.Equals(invisible))
                        {
                            selectedAbilityName = null;
                        }
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '3')
                    {
                        DisableButtonServerRpc(magnetism);
                        if (selectedAbilityName != null && selectedAbilityName.Equals(magnetism))
                        {
                            selectedAbilityName = null;
                        }
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '4')
                    {
                        DisableButtonServerRpc(flight);
                        if (selectedAbilityName != null && selectedAbilityName.Equals(flight))
                        {
                            selectedAbilityName = null;
                        }
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '5')
                    {
                        DisableButtonServerRpc(surveillance);
                        if (selectedAbilityName != null && selectedAbilityName.Equals(surveillance))
                        {
                            selectedAbilityName = null;
                        }
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '6')
                    {
                        DisableButtonServerRpc(taser);
                        if (selectedAbilityName != null && selectedAbilityName.Equals(taser))
                        {
                            selectedAbilityName = null;
                        }
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '7')
                    {
                        DisableButtonServerRpc(thermalGlass);
                        if (selectedAbilityName != null && selectedAbilityName.Equals(thermalGlass))
                        {
                            selectedAbilityName = null;
                        }
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '8')
                    {
                        DisableButtonServerRpc(luope);
                        if (selectedAbilityName != null && selectedAbilityName.Equals(luope))
                        {
                            selectedAbilityName = null;
                        }
                    }
                }
            }
        }
    }

    public void SetSelectedAbility(string abilityName)
    {
        if (IsOwner)
        {
            selectedAbilityName = abilityName;
            if (selectedAbilityName.Equals(superStrength))
            {
                transform.GetChild(1).GetChild(4).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                transform.GetChild(1).GetChild(4).GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(4).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(5).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                Desc1.text = flashDesc;
            }
            else if (selectedAbilityName.Equals(invisible))
            {
                transform.GetChild(1).GetChild(4).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                transform.GetChild(1).GetChild(4).GetChild(4).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(5).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                Desc1.text = invisibleDesc;
            }
            else if (selectedAbilityName.Equals(magnetism))
            {
                transform.GetChild(1).GetChild(4).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(4).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                transform.GetChild(1).GetChild(4).GetChild(5).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                Desc1.text = magnetismDesc;
            }
            else if (selectedAbilityName.Equals(flight))
            {
                transform.GetChild(1).GetChild(4).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(4).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(4).GetChild(5).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                Desc1.text = flightDesc;
            }
            else if (selectedAbilityName.Equals(surveillance))
            {
                transform.GetChild(1).GetChild(5).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                transform.GetChild(1).GetChild(5).GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(4).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(5).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                Desc2.text = cctvDesc;
            }
            else if (selectedAbilityName.Equals(taser))
            {
                transform.GetChild(1).GetChild(5).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                transform.GetChild(1).GetChild(5).GetChild(4).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(5).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                Desc2.text = taserDesc;
            }
            else if (selectedAbilityName.Equals(thermalGlass))
            {
                transform.GetChild(1).GetChild(5).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(4).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                transform.GetChild(1).GetChild(5).GetChild(5).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                Desc2.text = thermalDesc;
            }
            else if (selectedAbilityName.Equals(luope))
            {
                transform.GetChild(1).GetChild(5).GetChild(2).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(3).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(4).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1);
                transform.GetChild(1).GetChild(5).GetChild(5).GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1);
                Desc2.text = loupeDesc;
            }
        }
    }

    public void SetAbility()
    {
        if (IsOwner)
        {
            if (!string.IsNullOrEmpty(selectedAbilityName))
            {
                SetSelectedAbilityServerRpc(selectedAbilityName);
                gameObject.transform.GetChild(1).GetChild(4).GetChild(7).gameObject.SetActive(false);
                gameObject.transform.GetChild(1).GetChild(5).GetChild(7).gameObject.SetActive(false);
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void SetSelectedAbilityServerRpc(string abilityName)
    {
        Abilities selectedAbility = null;
        foreach (var ability in abilities)
        {
            if (ability.name == abilityName)
            {
                selectedAbility = ability;
                break;
            }
        }

        if (selectedAbility != null)
        {
            selfAbility.Value = new AbilityData
            {
                name = selectedAbility.name,
                cdTime = selectedAbility.cdTime,
                activeTime = selectedAbility.activeTime
            };

            appendCharServerRpc(selectedAbility.name);

            DisableButtonServerRpc(selectedAbility.name);
            playersReadyCount.Value++;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void DisableButtonServerRpc(string name)
    {
        DisableButtonClientRpc(name);
    }

    [ClientRpc]
    private void DisableButtonClientRpc(string name)
    {
        if (name.Equals(superStrength))
        {
            gameObject.transform.GetChild(1).GetChild(4).GetChild(2).GetComponent<Button>().interactable = false;
            gameObject.transform.GetChild(1).GetChild(4).GetChild(2).GetComponent<MainMenuBtn>().enabled = false;
            gameObject.transform.GetChild(1).GetChild(4).GetChild(2).GetChild(0).GetComponent<Image>().color = Color.grey;
        }
        else if (name.Equals(invisible))
        {
            gameObject.transform.GetChild(1).GetChild(4).GetChild(3).GetComponent<Button>().interactable = false;
            gameObject.transform.GetChild(1).GetChild(4).GetChild(3).GetComponent<MainMenuBtn>().enabled = false;
            gameObject.transform.GetChild(1).GetChild(4).GetChild(3).GetChild(0).GetComponent<Image>().color = Color.grey;
        }
        else if (name.Equals(magnetism))
        {
            gameObject.transform.GetChild(1).GetChild(4).GetChild(4).GetComponent<Button>().interactable = false;
            gameObject.transform.GetChild(1).GetChild(4).GetChild(4).GetComponent<MainMenuBtn>().enabled = false;
            gameObject.transform.GetChild(1).GetChild(4).GetChild(4).GetChild(0).GetComponent<Image>().color = Color.grey;
        }
        else if (name.Equals(flight))
        {
            gameObject.transform.GetChild(1).GetChild(4).GetChild(5).GetComponent<Button>().interactable = false;
            gameObject.transform.GetChild(1).GetChild(4).GetChild(5).GetComponent<MainMenuBtn>().enabled = false;
            gameObject.transform.GetChild(1).GetChild(4).GetChild(5).GetChild(0).GetComponent<Image>().color = Color.grey;
        }
        else if (name.Equals(surveillance))
        {
            gameObject.transform.GetChild(1).GetChild(5).GetChild(2).GetComponent<Button>().interactable = false;
            gameObject.transform.GetChild(1).GetChild(5).GetChild(2).GetComponent<MainMenuBtn>().enabled = false;
            gameObject.transform.GetChild(1).GetChild(5).GetChild(2).GetChild(0).GetComponent<Image>().color = Color.grey;
        }
        else if (name.Equals(taser))
        {
            gameObject.transform.GetChild(1).GetChild(5).GetChild(3).GetComponent<Button>().interactable = false;
            gameObject.transform.GetChild(1).GetChild(5).GetChild(3).GetComponent<MainMenuBtn>().enabled = false;
            gameObject.transform.GetChild(1).GetChild(5).GetChild(3).GetChild(0).GetComponent<Image>().color = Color.grey;
        }
        else if (name.Equals(thermalGlass))
        {
            gameObject.transform.GetChild(1).GetChild(5).GetChild(4).GetComponent<Button>().interactable = false;
            gameObject.transform.GetChild(1).GetChild(5).GetChild(4).GetComponent<MainMenuBtn>().enabled = false;
            gameObject.transform.GetChild(1).GetChild(5).GetChild(4).GetChild(0).GetComponent<Image>().color = Color.grey;
        }
        else if (name.Equals(luope))
        {
            gameObject.transform.GetChild(1).GetChild(5).GetChild(5).GetComponent<Button>().interactable = false;
            gameObject.transform.GetChild(1).GetChild(5).GetChild(5).GetComponent<MainMenuBtn>().enabled = false;
            gameObject.transform.GetChild(1).GetChild(5).GetChild(5).GetChild(0).GetComponent<Image>().color = Color.grey;
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void setAllLoadedServerRpc(bool value)
    {
        allLoaded.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    private void incTeam1ServerRpc()
    {
        team1Count.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void incTeam2ServerRpc()
    {
        team2Count.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void incPlayerServerRpc()
    {
        playerCount.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    private void appendCharServerRpc(string selectedAbility)
    {
        if (selectedAbility.Equals(superStrength))
        {
            chosenAbility.Value.chosenAbilityEnum += '1';
        }
        else if (selectedAbility.Equals(invisible))
        {
            chosenAbility.Value.chosenAbilityEnum += '2';
        }
        else if (selectedAbility.Equals(magnetism))
        {
            chosenAbility.Value.chosenAbilityEnum += '3';
        }
        else if (selectedAbility.Equals(flight))
        {
            chosenAbility.Value.chosenAbilityEnum += '4';
        }
        else if (selectedAbility.Equals(surveillance))
        {
            chosenAbility.Value.chosenAbilityEnum += '5';
        }
        else if (selectedAbility.Equals(taser))
        {
            chosenAbility.Value.chosenAbilityEnum += '6';
        }
        else if (selectedAbility.Equals(thermalGlass))
        {
            chosenAbility.Value.chosenAbilityEnum += '7';
        }
        else if (selectedAbility.Equals(luope))
        {
            chosenAbility.Value.chosenAbilityEnum += '8';
        }
    }

    private IEnumerator Wait()
    {
        gameObject.transform.position = GetRandomSpawnPoint();
        yield return new WaitForSeconds(1);
        gameObject.transform.GetChild(1).GetChild(4).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).GetChild(5).gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (IsOwner)
        {
            gameObject.GetComponent<PlayerController>().enabled = true;
            gameObject.GetComponent<MissionManager>().enabled = true;
            gameObject.GetComponent<AbilityHolder>().enabled = true;
            gameObject.GetComponent<EliminateSuperNormies>().enabled = true;
            gameObject.GetComponent<BoxCollider>().enabled = true;
            gameObject.GetComponent<TimerDisplay>().enabled = true;
            gameObject.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
            gameObject.transform.GetChild(1).GetChild(2).gameObject.SetActive(true);
            if (isPresent())
                transform.GetChild(10).gameObject.SetActive(true);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.IsServer || LobbyManager.Instance.IsLobbyHost())
            if(NetworkManager.Singleton.ConnectedClients.Count == 1)
                InitGameServerRpc();

        playerCount.OnValueChanged += OnPlayerValueChange;
        allLoaded.OnValueChanged += OnAllLoadedValueChange;
        team1Count.OnValueChanged += OnTeam1ValueChange;
        team2Count.OnValueChanged += OnTeam2ValueChange;
        team1Disabled.OnValueChanged += OnTeam1DisabledChange;
        team2Disabled.OnValueChanged += OnTeam2DisabledChange;
        playersReadyCount.OnValueChanged += OnPlayerReadyCountChange;
        selfAbility.OnValueChanged += (AbilityData oldValue, AbilityData newValue) => { };
        chosenAbility.OnValueChanged += OnChosenAbilityChange;

        if (IsOwner)
        {
            incPlayerServerRpc();
            if (chosenAbility.Value.chosenAbilityEnum != null)
            {
                for (int i = 0; i < chosenAbility.Value.chosenAbilityEnum.Length; i++)
                {
                    if (chosenAbility.Value.chosenAbilityEnum[i] == '1')
                    {
                        DisableButtonServerRpc(superStrength);
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '2')
                    {
                        DisableButtonServerRpc(invisible);
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '3')
                    {
                        DisableButtonServerRpc(magnetism);
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '4')
                    {
                        DisableButtonServerRpc(flight);
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '5')
                    {
                        DisableButtonServerRpc(surveillance);
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '6')
                    {
                        DisableButtonServerRpc(taser);
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '7')
                    {
                        DisableButtonServerRpc(thermalGlass);
                    }
                    else if (chosenAbility.Value.chosenAbilityEnum[i] == '8')
                    {
                        DisableButtonServerRpc(luope);
                    }
                }
            }
        }
            

        
    }

    public override void OnNetworkDespawn()
    {
        playerCount.OnValueChanged -= OnPlayerValueChange;
        allLoaded.OnValueChanged -= OnAllLoadedValueChange;
        team1Count.OnValueChanged -= OnTeam1ValueChange;
        team2Count.OnValueChanged -= OnTeam2ValueChange;
        team1Disabled.OnValueChanged -= OnTeam1DisabledChange;
        team2Disabled.OnValueChanged -= OnTeam2DisabledChange;
        playersReadyCount.OnValueChanged -= OnPlayerReadyCountChange;
        chosenAbility.OnValueChanged -= OnChosenAbilityChange;
    }

    public Abilities GetAbilities()
    {
        if (abilities == null) 
        { 
            abilities = GetComponent<AbilityHolder>().ability; 
        }

        foreach (var ability in abilities) 
        { 
            if (ability.name == selfAbility.Value.name) 
            { 
                return ability; 
            } 
        }

        return null;
    }

    [ServerRpc (RequireOwnership = false)]
    private void InitGameServerRpc()
    {
        team1Count.Value = 0;
        team2Count.Value = 0;
        team1Disabled.Value = false;
        team2Disabled.Value = false;
        playerCount.Value = 0;
        allLoaded.Value = false;
        playersReadyCount.Value = 0;
        chosenAbility.Value = new StringSerializer();
    }

    private Vector3 GetRandomSpawnPoint()
    {
        Vector3 randomSpawnPoint = new Vector3(0, 0, 0);
        int maxAttempts = 10;
        int attempts = 0;
        bool validSpawnPointFound = false;
        LayerMask obstacleLayerMask = LayerMask.GetMask("Obstacle");

        while (attempts < maxAttempts && !validSpawnPointFound)
        {
            float randomX = UnityEngine.Random.Range(mapMinBounds.x, mapMaxBounds.x);
            float randomY = UnityEngine.Random.Range(mapMinBounds.y, mapMaxBounds.y);
            float randomZ = UnityEngine.Random.Range(mapMinBounds.z, mapMaxBounds.z);
            randomSpawnPoint = new Vector3(randomX, randomY, randomZ);

            if (!Physics.CheckBox(randomSpawnPoint, new Vector3(0.5f, 0.5f, 0.5f), Quaternion.identity, obstacleLayerMask))
            {
                validSpawnPointFound = true;
            }
            attempts++;
        }

        if (!validSpawnPointFound)
        {
            // Fallback in case no valid spawn point is found
            randomSpawnPoint = new Vector3((mapMinBounds.x + mapMaxBounds.x) / 2, mapMaxBounds.y, (mapMinBounds.z + mapMaxBounds.z) / 2);
        }

        return randomSpawnPoint;
    }


    public static bool isPresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                return true;
            }
        }
        return false;
    }

}