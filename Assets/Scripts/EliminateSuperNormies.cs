using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class EliminateSuperNormies : NetworkBehaviour
{
    private string superStrength = "SuperStrength";
    private string invisible = "Invisible";
    private string flight = "Flight";
    private string magnetism = "Magnetism";
    private string surveillance = "Surveillance";
    private string taser = "Taser";
    private string thermalGlass = "ThermalGlass";
    private string luope = "Luope";
    private string player = "Player";
    private string npc = "NPC";
    private string msg = "Press F to Eliminate This Character";
    private string msgXR = "Press Left Trigger to Eliminate This Character";
    private string eliminated = "You have been Eliminated.";
    private string abilityName;
    private bool enter = false;

    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();
    private GameObject playerCharacter;

    private void Start()
    {
        abilityName = GetComponent<PlayerManager>().GetAbilities().name;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GetComponent<PlayerController>().IsLocalPlayer && (abilityName.Equals(surveillance) || abilityName.Equals(taser) || abilityName.Equals(thermalGlass) || abilityName.Equals(luope)))
        {
            if (other.tag.Equals(player) || other.tag.Equals(npc))
            {
                playersInTrigger.Add(other.gameObject);
                playerCharacter = other.gameObject;
                transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(true);
                if(!isPresent())
                    transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msg;
                else
                    transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msgXR;
                enter = true;
            }
        }
    }

    public void EliminateTriggerPressedXR(InputAction.CallbackContext context)
    {
        if (enter)
        {
            if (playersInTrigger.Contains(playerCharacter))
            {
                if (playerCharacter.tag.Equals(player) && playerCharacter != null)
                {
                    if (context.performed && (playerCharacter.GetComponent<PlayerManager>().GetAbilities().name.Equals(superStrength) || playerCharacter.GetComponent<PlayerManager>().GetAbilities().name.Equals(invisible) || playerCharacter.GetComponent<PlayerManager>().GetAbilities().name.Equals(flight) || playerCharacter.GetComponent<PlayerManager>().GetAbilities().name.Equals(magnetism)))
                    {
                        EliminatePlayerServerRpc(playerCharacter.GetComponent<NetworkObject>().NetworkObjectId);
                        playersInTrigger.Remove(playerCharacter);
                        enter = false;
                        gameObject.GetComponent<MissionManager>().incNormiesCount();
                    }
                }
                else if (playerCharacter.tag.Equals(npc))
                {
                    if (context.performed)
                    {
                        //playersInTrigger.Remove(playerCharacter);
                        enter = false;
                        gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                        gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                        gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                        gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (enter)
        {
            if (playersInTrigger.Contains(playerCharacter))
            {
                if (playerCharacter.tag.Equals(player) && playerCharacter != null)
                {
                    if (Input.GetKeyDown(KeyCode.F) && (playerCharacter.GetComponent<PlayerManager>().GetAbilities().name.Equals(superStrength) || playerCharacter.GetComponent<PlayerManager>().GetAbilities().name.Equals(invisible) || playerCharacter.GetComponent<PlayerManager>().GetAbilities().name.Equals(flight) || playerCharacter.GetComponent<PlayerManager>().GetAbilities().name.Equals(magnetism)))
                    {
                        EliminatePlayerServerRpc(playerCharacter.GetComponent<NetworkObject>().NetworkObjectId);
                        playersInTrigger.Remove(playerCharacter);
                        enter = false;
                        gameObject.GetComponent<MissionManager>().incNormiesCount();
                    }
                }
                else if (playerCharacter.tag.Equals(npc))
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        //playersInTrigger.Remove(playerCharacter);
                        enter = false;
                        gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                        gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                        gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                        gameObject.GetComponent<MissionManager>().incSuperNormiesCount();
                    }
                }
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void EliminatePlayerServerRpc(ulong playerID)
    {
        EliminatePlayerClientRpc(playerID);
    }

    [ClientRpc]
    private void EliminatePlayerClientRpc(ulong playerID)
    {
        NetworkObject playerCharacter = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerID];
        GameObject target = playerCharacter.gameObject;
        target.GetComponent<PlayerManager>().enabled = false;
        target.GetComponent<AbilityHolder>().enabled = false;
        target.GetComponent<MissionManager>().Eliminated();
        target.transform.GetChild(4).gameObject.SetActive(false);
        target.GetComponent<CharacterController>().radius = 0;
        target.GetComponent<PlayerController>().gravity = 0;
        target.GetComponent<PlayerController>().moveDirection.y = 0;
        if (target.GetComponent<PlayerController>().IsLocalPlayer)
        {
            Vector3 newPosition = new Vector3(0, 20, 0);
            target.transform.position = newPosition;
            target.transform.GetChild(1).GetChild(2).GetChild(8).gameObject.SetActive(false);
            target.transform.GetChild(1).GetChild(2).GetChild(9).GetComponent<TextMeshProUGUI>().text = eliminated;
            target.transform.GetChild(1).GetChild(2).GetChild(10).gameObject.SetActive(false);
            target.transform.GetChild(1).GetChild(2).GetChild(11).gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals(player) || other.tag.Equals(npc))
        {
            playersInTrigger.Remove(other.gameObject);
            transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(false);
            transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
            enter = false;
            if (other.gameObject == playerCharacter)
            {
                playerCharacter = null;
            }
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
