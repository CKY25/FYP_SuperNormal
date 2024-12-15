using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class MissionManager : NetworkBehaviour
{
    public GameObject cupPrefab;
    public TextMeshProUGUI missionText;
    public TextMeshProUGUI missionText2;
    public AudioClip victory;
    private Dictionary<string, Mission> missions = new Dictionary<string, Mission>();
    private PlayerManager playerManager;
    private Dictionary<string, Coroutine> activeMissionCoroutines = new Dictionary<string, Coroutine>();

    private static string player = "Player";
    private string superStrength = "SuperStrength";
    private string invisible = "Invisible";
    private string flight = "Flight";
    private string magnetism = "Magnetism";
    private string surveillance = "Surveillance";
    private string taser = "Taser";
    private string thermalGlass = "ThermalGlass";
    private string luope = "Luope";
    private string missionString;
    private string missionString2;
    private string secondsLeft = " seconds left.";
    const string STRIKE_START = "<s>";
    const string STRIKE_END = "</s>";
    const string SLASH = "/";
    private static string invisibleMission = "Stay Invisible for Total 3 Minutes";
    private static string flightMission = "Stay Flying for 30 Seconds Straight.";
    private static string superStrengthMission = "Hand Over A Cup of Water to An NPC.";
    private static string magnetismMission = "Reposition Back 4 Metal Armour";
    private static string sitChairMission = "Sit on Chairs for Total 3 Minutes.";
    private static string stayUnderwaterMission = "Stay Under Water for Total 30 Seconds.";
    private static string normiesMission = "Eliminate All The SuperNormies.";
    private static string talkMission = "Talk With 3 NPCs.";
    private static string normiesWin = "Normies Won The Game!";
    private static string superNormiesWin = "SuperNormies Won The Game!";

    private int talkNPCCount = 0;
    private int passWaterCount = 0;
    public static NetworkVariable<int> superNormiesMissionCount = new NetworkVariable<int>(0);
    public static NetworkVariable<int> normiesMissionCount =  new NetworkVariable<int>(0);
    private int metalArmour = 0;
    private int connectedClients = 0;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        InitializeMissions();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count == 1)
            {
                superNormiesMissionCount.Value = 0;
                normiesMissionCount.Value = 0;
            }   
        }

        superNormiesMissionCount.OnValueChanged += OnSuperNormiesMissionCountChanged;
        normiesMissionCount.OnValueChanged += OnNormiesMissionCountChanged;
    }

    public override void OnNetworkDespawn()
    {
        superNormiesMissionCount.OnValueChanged -= OnSuperNormiesMissionCountChanged;
        normiesMissionCount.OnValueChanged -= OnNormiesMissionCountChanged;
    }

    private void InitializeMissions()
    {
        if (!IsOwner) return;
        if (playerManager.GetAbilities().name.Equals(invisible))
        {
            AddMission(new Mission(invisibleMission, 180f), missionText);
            missionText.text = "\n" + invisibleMission + "\t";
            missionString = missionText.text;
        }
        else if (playerManager.GetAbilities().name.Equals(flight))
        {
            AddMission(new Mission(flightMission, 30f), missionText);
            missionText.text = "\n" + flightMission + "\t";
            missionString = missionText.text;
        }
        else if (playerManager.GetAbilities().name.Equals(superStrength))
        {
            AddMission(new Mission(superStrengthMission, 1f), missionText);
            missionText.text = "\n" + superStrengthMission + "\t";
            missionString = missionText.text;
            instantiateCupServerRpc();
        }
        else if (playerManager.GetAbilities().name.Equals(magnetism))
        {
            AddMission(new Mission(magnetismMission, 4f), missionText);
            missionText.text = "\n" + magnetismMission + "\t";
            missionString = missionText.text;
        }

        if (playerManager.GetAbilities().name.Equals(superStrength) || playerManager.GetAbilities().name.Equals(invisible) || playerManager.GetAbilities().name.Equals(flight) || playerManager.GetAbilities().name.Equals(magnetism))
        {
            int rand = UnityEngine.Random.Range(1, 1001);
            if (rand < 251)
            {
                AddMission(new Mission(sitChairMission, 180f), missionText2);
                missionText2.text = "\n" + sitChairMission + "\t";
                missionString2 = missionText2.text;
            }
            else if (rand < 501)
            {
                AddMission(new Mission(stayUnderwaterMission, 30f), missionText2);
                missionText2.text = "\n" + stayUnderwaterMission + "\t";
                missionString2 = missionText2.text;
            }
            else
            {
                AddMission(new Mission(talkMission, 3), missionText2);
                missionText2.text = "\n" + talkMission + "\t";
                missionString2 = missionText2.text;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(surveillance) || playerManager.GetAbilities().name.Equals(taser) || playerManager.GetAbilities().name.Equals(thermalGlass) || playerManager.GetAbilities().name.Equals(luope))
        {
            AddMission(new Mission(normiesMission, 0f), missionText);
            missionText.text = "\n" + normiesMission + "\t";
            missionString = missionText.text;
        }
    }

    public void AddMission(Mission mission, TextMeshProUGUI missionText)
    {
        missions.Add(mission.TaskDescription, mission);
        if (!activeMissionCoroutines.ContainsKey(mission.TaskDescription)) 
        { 
            activeMissionCoroutines[mission.TaskDescription] = StartCoroutine(TrackMission(mission, missionText)); 
        }
    }

    public void Eliminated()
    {
        missions.Clear();
        activeMissionCoroutines.Clear();
        StopAllCoroutines();
    }

    private void OnSuperNormiesMissionCountChanged(int oldValue, int newValue)
    {
        if (IsServer || LobbyManager.Instance.IsLobbyHost())
        {
            connectedClients = NetworkManager.Singleton.ConnectedClients.Count;
            if (superNormiesMissionCount.Value >= connectedClients)
            {
                ShowGameCompleteUI_ServerRpc(superNormiesWin);
                if (isPresent())
                    transform.GetChild(10).gameObject.SetActive(false);
            }
        }
    }

    private void OnNormiesMissionCountChanged(int oldValue, int newValue)
    {
        if (IsServer || LobbyManager.Instance.IsLobbyHost())
        {
            connectedClients = NetworkManager.Singleton.ConnectedClients.Count;
            if (normiesMissionCount.Value >= connectedClients / 2)
            {
                ShowGameCompleteUI_ServerRpc(normiesWin);
                if (isPresent())
                    transform.GetChild(10).gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            foreach (var mission in missions.Values)
            {
                if (!mission.IsMissionComplete)
                {
                    if (mission.IsMissionComplete && activeMissionCoroutines.ContainsKey(mission.TaskDescription))
                    {
                        StopCoroutine(activeMissionCoroutines[mission.TaskDescription]);
                        activeMissionCoroutines.Remove(mission.TaskDescription);
                    }
                }
            }
        }
        
    }

    private IEnumerator TrackMission(Mission mission, TextMeshProUGUI missionText)
    {
        float remainingTime = mission.Duration;
        if (playerManager.GetAbilities().name.Equals(invisible) && mission.TaskDescription.Equals(invisibleMission))
        {
            while (remainingTime > 0)
            {
                missionText.text = missionString + remainingTime.ToString("0") + secondsLeft;
                if (!transform.GetChild(4).gameObject.activeSelf)
                {
                    remainingTime -= Time.deltaTime;
                }
                yield return null;
            }
            incSuperNormiesCount();
        }
        else if (playerManager.GetAbilities().name.Equals(flight) && mission.TaskDescription.Equals(flightMission))
        {
            while (remainingTime > 0)
            {
                missionText.text = missionString + remainingTime.ToString("0") + secondsLeft;
                if (!isPresent())
                {
                    if (GetComponent<PlayerController>().gravity == 0)
                    {
                        remainingTime -= Time.deltaTime;
                    }
                    else
                    {
                        remainingTime = mission.Duration;
                    }
                }
                else
                {
                    if (!transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().useGravity)
                    {
                        remainingTime -= Time.deltaTime;
                    }
                    else
                    {
                        remainingTime = mission.Duration;
                    }
                }
                
                yield return null;
            }
            incSuperNormiesCount();
        }
        else if (playerManager.GetAbilities().name.Equals(superStrength) && mission.TaskDescription.Equals(superStrengthMission))
        {
            while (passWaterCount < remainingTime)
            {
                missionText.text = missionString + passWaterCount + SLASH + remainingTime.ToString("0");
                yield return null;
            }
            incSuperNormiesCount();
        }
        else if (playerManager.GetAbilities().name.Equals(magnetism) && mission.TaskDescription.Equals(magnetismMission))
        {
            while (metalArmour < remainingTime)
            {
                missionText.text = missionString + metalArmour + SLASH + remainingTime.ToString("0");
                yield return null;
            }
            incSuperNormiesCount();
        }
        else if (mission.TaskDescription.Equals(sitChairMission))
        {
            while (remainingTime > 0)
            {
                missionText.text = missionString2 + remainingTime.ToString("0") + secondsLeft;
                if (transform.GetComponent<PlayerController>().isSittingMission)
                {
                    remainingTime -= Time.deltaTime;
                }
                yield return null;
            }
            incSuperNormiesCount();
        }
        else if (mission.TaskDescription.Equals(stayUnderwaterMission))
        {
            while (remainingTime > 0)
            {
                missionText.text = missionString2 + remainingTime.ToString("0") + secondsLeft;
                if (GetComponent<PlayerController>().isUnderWater)
                {
                    remainingTime -= Time.deltaTime;
                }
                yield return null;
            }
            incSuperNormiesCount();
        }
        else if (mission.TaskDescription.Equals(talkMission))
        {
            while (talkNPCCount < remainingTime)
            {
                missionText.text = missionString2 + talkNPCCount + SLASH + remainingTime.ToString("0");
                yield return null;
            }
            incSuperNormiesCount();
        }
        else if (mission.TaskDescription.Equals(normiesMission))
        {
            while (normiesMissionCount.Value < connectedClients / 2)
            {
                yield return null;
            }
        }

        mission.CompleteMission();
        missionText.text = STRIKE_START + mission.TaskDescription + STRIKE_END;
    }

    public void doneNPCTalk()
    {
        talkNPCCount++;
    }

    public void metalObjectIncrement()
    {
        metalArmour++;
    }

    public void passWaterIncrement()
    {
        passWaterCount++;
    }

    public void incNormiesCount()
    {
        normiesIncrementServerRpc();
    }

    [ServerRpc]
    public void normiesIncrementServerRpc()
    {
        normiesMissionCount.Value++;
        OnNormiesMissionCountChanged(0, normiesMissionCount.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void instantiateCupServerRpc()
    {
        var newmug = Instantiate(cupPrefab, gameObject.transform);
        newmug.transform.position = new Vector3(16.47f, -1.1f, -2.017f);
        newmug.GetComponent<PassCup>().enabled = true;
        newmug.GetComponent<NetworkObject>().enabled = true;
        newmug.GetComponent<ClientNetworkTransform>().enabled = true;
        newmug.GetComponent<NetworkObject>().Spawn();
    }

    public void incSuperNormiesCount()
    {
        superNormiesDoneMissionServerRpc();
    }

    [ServerRpc]
    public void superNormiesDoneMissionServerRpc()
    {
        superNormiesMissionCount.Value++;
        OnSuperNormiesMissionCountChanged(0, superNormiesMissionCount.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowGameCompleteUI_ServerRpc(string text)
    {
        try
        {
            ShowGameCompleteUI_ClientRpc(text);
        }
        catch(NullReferenceException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    [ClientRpc]
    private void ShowGameCompleteUI_ClientRpc(string text)
    {
        GameObject[] playerArr = GameObject.FindGameObjectsWithTag(player);
        foreach (var player in playerArr)
        {
            player.transform.GetChild(0).gameObject.SetActive(false);
            player.transform.GetChild(1).GetChild(3).gameObject.SetActive(true);
            player.transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = text;
            player.GetComponent<PlayerController>().enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            string ability = player.GetComponent<PlayerManager>().GetAbilities().name;
            AudioSource source = player.transform.GetChild(1).GetChild(3).GetComponent<AudioSource>();
            source.clip = victory;
            source.Play();

            player.transform.position = CustomNetworkManager.Instance.GetRandomSpawnPoint();
        }
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
