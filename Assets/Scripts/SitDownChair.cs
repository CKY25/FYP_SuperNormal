using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class SitDownChair : NetworkBehaviour
{
    private string player = "Player";
    private string npc = "NPC";
    private string isSitting = "isSitting";
    private string isWalking = "isWalking";
    private string isFloating = "isFloating";
    private string isJumping = "isJumping";
    private string superStrength = "SuperStrength";
    private string invisible = "Invisible";
    private string flight = "Flight";
    private string magnetism = "Magnetism";
    private string msg = "Press F to Sit Down";
    private string msg2 = "Press F to Stand Up";
    private string msgXR = "Press Left Trigger to Sit Down";
    private string msgXR2 = "Press Left Trigger to Stand Up";
    private NetworkVariable<bool> isOccupied = new NetworkVariable<bool>(false);
    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();
    private GameObject playerCharacter;
    private GameObject nonPlayerCharacter;
    private bool isRunning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isOccupied.Value)
        {
            if (other.gameObject.tag.Equals(player) && other.GetComponent<PlayerController>().IsLocalPlayer)
            {
                string abilityName = other.GetComponent<PlayerManager>().GetAbilities().name;
                if (abilityName.Equals(superStrength) || abilityName.Equals(invisible) || abilityName.Equals(flight) || abilityName.Equals(magnetism))
                {
                    playersInTrigger.Add(other.gameObject);
                    playerCharacter = other.gameObject;
                    other.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(true);
                    if(!isPresent())
                        other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msg;
                    else
                        other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msgXR;
                }
            }
            else if (other.gameObject.tag.Equals(npc))
            {
                playersInTrigger.Add(other.gameObject);
                nonPlayerCharacter = other.gameObject;
                int rand = Random.Range(0, 10);
                if (rand == 5)
                {
                    UpdateActivatedServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, true);
                    UpdateOccupancyServerRpc(true);
                    UpdateNPCAnimatorServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, isSitting, true);
                    nonPlayerCharacter.transform.position = gameObject.transform.position;
                    nonPlayerCharacter.transform.rotation = gameObject.transform.rotation;
                    nonPlayerCharacter.transform.position = gameObject.transform.position;
                    nonPlayerCharacter.transform.rotation = gameObject.transform.rotation;
                }
            }
        }
    }

    public void SitTriggerPressedXR(InputAction.CallbackContext context)
    {
        if (playersInTrigger.Contains(playerCharacter))
        {
            if (!isOccupied.Value)
            {
                if (playerCharacter.tag.Equals(player) && !playerCharacter.GetComponent<PlayerController>().isSittingMission)
                {
                    if (context.performed)
                    {
                        if (!isPresent())
                        {
                            playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msg2;
                            playerCharacter.GetComponent<PlayerController>().enabled = false;
                        }
                        else
                        {
                            playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msgXR2;
                            playerCharacter.transform.GetChild(10).gameObject.SetActive(false);
                        }

                        playerCharacter.GetComponent<PlayerController>().isSittingMission = true;
                        playerCharacter.transform.position = gameObject.transform.position;
                        playerCharacter.transform.rotation = gameObject.transform.rotation;
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isWalking, false);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isFloating, false);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isJumping, false);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isSitting, true);
                        UpdateOccupancyServerRpc(true);
                    }
                }
            }
            else if (isOccupied.Value)
            {
                if (playerCharacter.tag.Equals(player) && playerCharacter.GetComponent<PlayerController>().isSittingMission)
                {
                    if (context.performed)
                    {
                        if (!isPresent())
                        {
                            playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msg;
                            playerCharacter.GetComponent<PlayerController>().enabled = true;
                        }
                        else
                        {
                            playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msgXR;
                            playerCharacter.transform.GetChild(10).gameObject.SetActive(true);
                        }

                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isSitting, false);

                        playerCharacter.GetComponent<PlayerController>().isSittingMission = false;
                        UpdateOccupancyServerRpc(false);
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (playersInTrigger.Contains(playerCharacter))
        {
            if (!isOccupied.Value)
            {
                if (playerCharacter.tag.Equals(player) && !playerCharacter.GetComponent<PlayerController>().isSittingMission)
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (!isPresent())
                        {
                            playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msg2;
                            playerCharacter.GetComponent<PlayerController>().enabled = false;
                        }
                        else
                        {
                            playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msgXR2;
                            playerCharacter.transform.GetChild(10).gameObject.SetActive(false);
                        }
                            
                        playerCharacter.GetComponent<PlayerController>().isSittingMission = true;
                        playerCharacter.transform.position = gameObject.transform.position;
                        playerCharacter.transform.rotation = gameObject.transform.rotation;
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isWalking, false);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isFloating, false);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isJumping, false);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isSitting, true);
                        UpdateOccupancyServerRpc(true);
                    }
                }
            }
            else if (isOccupied.Value)
            {
                if (playerCharacter.tag.Equals(player) && playerCharacter.GetComponent<PlayerController>().isSittingMission)
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        if (!isPresent())
                        {
                            playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msg;
                            playerCharacter.GetComponent<PlayerController>().enabled = true;
                        }
                        else
                        {
                            playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msgXR;
                            playerCharacter.transform.GetChild(10).gameObject.SetActive(true);
                        }
                        
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isSitting, false);
                        
                        playerCharacter.GetComponent<PlayerController>().isSittingMission = false;
                        UpdateOccupancyServerRpc(false);
                    }
                }
            }
        }
        else if (playersInTrigger.Contains(nonPlayerCharacter))
        {
            if (isOccupied.Value)
            {
                if (nonPlayerCharacter.GetComponent<Animator>().GetBool(isSitting))
                {
                    if(!isRunning)
                        StartCoroutine(getUp());
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals(player))
        {
            playersInTrigger.Remove(other.gameObject);
            other.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(false);
            other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
            if (other.gameObject == playerCharacter && !isOccupied.Value) 
            { 
                playerCharacter = null; 
            }
        }
        else if (other.gameObject.tag.Equals(npc))
        {
            playersInTrigger.Remove(other.gameObject);
            if (other.gameObject == nonPlayerCharacter && !isOccupied.Value)
            {
                nonPlayerCharacter = null;
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void UpdateOccupancyServerRpc(bool state) 
    {
        isOccupied.Value = state;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateActivatedServerRpc(ulong targetNetworkObjectId, bool state)
    {
        UpdateActivatedClientRpc(targetNetworkObjectId, state);
    }

    [ClientRpc]
    private void UpdateActivatedClientRpc(ulong targetNetworkObjectId, bool state)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;

        if (target.GetComponent<NavMeshAgent>().isActiveAndEnabled)
            target.GetComponent<NavMeshAgent>().isStopped = state;
        target.GetComponent<NavMeshAgent>().velocity = Vector3.zero;
        target.GetComponent<NavMeshAgent>().enabled = !state;
        target.GetComponent<Nav>().enabled = !state;
        target.GetComponent<TalkToNPC>().UpdateStateServerRpc(state);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateNPCAnimatorServerRpc(ulong targetNetworkObjectId, string animationName, bool state)
    {
        UpdateNPCAnimatorClientRpc(targetNetworkObjectId, animationName, state);
    }

    [ClientRpc]
    private void UpdateNPCAnimatorClientRpc(ulong targetNetworkObjectId, string animationName, bool state)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.GetComponent<Animator>().SetBool(animationName, state);
    }

    private IEnumerator getUp()
    {
        isRunning = true;
        yield return new WaitForSeconds(3);
        if (nonPlayerCharacter != null)
        {
            int rand = Random.Range(0, 10);
            if (rand == 5)
            {
                UpdateActivatedServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, false);
                UpdateOccupancyServerRpc(false);
                UpdateNPCAnimatorServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, isSitting, false);
            }
        }
        isRunning = false;
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
