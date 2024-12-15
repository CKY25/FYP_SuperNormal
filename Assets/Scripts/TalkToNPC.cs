using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using static UnityEngine.GraphicsBuffer;

public class TalkToNPC : NetworkBehaviour
{
    private string player = "Player";
    private string npc = "NPC";
    private string isWalking = "isWalking";
    private string isTalking1 = "isTalking1";
    private string isTalking2 = "isTalking2";
    private string superStrength = "SuperStrength";
    private string invisible = "Invisible";
    private string flight = "Flight";
    private string magnetism = "Magnetism";
    private string msg = "Press F to Talk to NPC";
    private string msg2 = "Press F to Quit Talking";
    private string msgXR = "Press Left Trigger to Talk to NPC";
    private string msgXR2 = "Press Left Trigger to Quit Talking";
    private NetworkVariable<bool> activated = new NetworkVariable<bool>(false);
    private HashSet<GameObject> playersInTrigger = new HashSet<GameObject>();
    private GameObject playerCharacter;
    private GameObject nonPlayerCharacter;
    private bool isRunning = false;
    private bool isRunning2 = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!activated.Value)
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
            else if (other.gameObject.tag.Equals(npc) && !other.GetComponent<TalkToNPC>().activated.Value)
            {
                playersInTrigger.Add(other.gameObject);
                nonPlayerCharacter = other.gameObject;
                if (!isRunning2)
                {
                    StartCoroutine(StartTalk());
                }
            }
        }
    }

    public void TalkTriggerPressedXR(InputAction.CallbackContext context)
    {
        if (playersInTrigger.Contains(playerCharacter))
        {
            if (!activated.Value)
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

                    playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isWalking, false);
                    Vector3 target = new Vector3(playerCharacter.transform.position.x, this.transform.position.y, playerCharacter.transform.position.z);
                    transform.LookAt(target);

                    int rand = Random.Range(0, 2);
                    if (rand == 0)
                    {
                        UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, true);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isTalking1, true);
                    }
                    else
                    {
                        UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, true);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isTalking2, true);
                    }

                    UpdateActivatedServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, true);
                    playerCharacter.GetComponent<MissionManager>().doneNPCTalk();
                }
            }
            else
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
                    UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, false);
                    UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, false);
                    playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isTalking1, false);
                    playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isTalking2, false);
                    UpdateActivatedServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, false);
                }
            }
        }
    }

    private void Update()
    {
        if (playersInTrigger.Contains(playerCharacter))
        {
            if (!activated.Value)
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
                        
                    playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isWalking, false);
                    Vector3 target = new Vector3(playerCharacter.transform.position.x, this.transform.position.y, playerCharacter.transform.position.z);
                    transform.LookAt(target);

                    int rand = Random.Range(0, 2);
                    if (rand == 0)
                    {
                        UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, true);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isTalking1, true);
                    }
                    else
                    {
                        UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, true);
                        playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isTalking2, true);
                    }

                    UpdateActivatedServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, true);
                    playerCharacter.GetComponent<MissionManager>().doneNPCTalk();
                }
            }
            else
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
                    UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, false);
                    UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, false);
                    playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isTalking1, false);
                    playerCharacter.transform.GetChild(4).GetComponent<Animator>().SetBool(isTalking2, false);
                    UpdateActivatedServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, false);
                }
            }
        }
        else if (playersInTrigger.Contains(nonPlayerCharacter))
        {
            if (activated.Value)
            {
                if (nonPlayerCharacter.GetComponent<Animator>().GetBool(isTalking1) || nonPlayerCharacter.GetComponent<Animator>().GetBool(isTalking2))
                {
                    if (!isRunning)
                        StartCoroutine(stopTalk());
                }
            }
        }

        if (playersInTrigger == null)
        {
            UpdateActivatedServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, false);
            UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, false);
            UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals(player))
        {
            playersInTrigger.Remove(other.gameObject);
            other.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(false);
            other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
            if (other.gameObject == playerCharacter && !activated.Value)
            {
                playerCharacter = null;
            }
        }
        else if (other.gameObject.tag.Equals(npc))
        {
            playersInTrigger.Remove(other.gameObject);
            if (other.gameObject == nonPlayerCharacter && !activated.Value)
            {
                nonPlayerCharacter = null;
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    public void UpdateActivatedServerRpc(ulong targetNetworkObjectId, bool state)
    {
        UpdateActivatedClientRpc(targetNetworkObjectId, state);
        activated.Value = state;
    }

    [ClientRpc]
    private void UpdateActivatedClientRpc(ulong targetNetworkObjectId, bool state)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;

        if(target.GetComponent<NavMeshAgent>().isActiveAndEnabled)
            target.GetComponent<NavMeshAgent>().isStopped = state;
        target.GetComponent<NavMeshAgent>().velocity = Vector3.zero;
        target.GetComponent<NavMeshAgent>().enabled = !state;
        target.GetComponent<Nav>().enabled = !state;
    }

    [ServerRpc (RequireOwnership = false)]
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

    private IEnumerator StartTalk()
    {
        isRunning2 = true;
        yield return new WaitForSeconds(1);
        if (nonPlayerCharacter != null)
        {
            int rand = Random.Range(0, 10);
            if (rand == 5)
            {
                int rand2 = Random.Range(0, 2);
                if (rand2 == 0)
                {
                    UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, true);
                    UpdateNPCAnimatorServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, true);
                }
                else
                {
                    UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, true);
                    UpdateNPCAnimatorServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, true);
                }

                Vector3 target1 = new Vector3(nonPlayerCharacter.transform.position.x, this.transform.position.y, nonPlayerCharacter.transform.position.z);
                transform.LookAt(target1);

                Vector3 target2 = new Vector3(transform.position.x, nonPlayerCharacter.transform.position.y, transform.position.z);
                nonPlayerCharacter.transform.LookAt(target2);

                UpdateActivatedServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, true);
                nonPlayerCharacter.GetComponent<TalkToNPC>().UpdateActivatedServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, true);
            }
        }
        isRunning2 = false;
    }

    private IEnumerator stopTalk()
    {
        isRunning = true;
        yield return new WaitForSeconds(3);
        if (nonPlayerCharacter != null)
        {
            int rand = Random.Range(0, 10);
            if (rand == 5)
            {
                UpdateActivatedServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, false);
                nonPlayerCharacter.GetComponent<TalkToNPC>().UpdateActivatedServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, false);
                UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, false);
                UpdateNPCAnimatorServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, false);
                UpdateNPCAnimatorServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, isTalking1, false);
                UpdateNPCAnimatorServerRpc(nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId, isTalking2, false);
            }
        }
        isRunning = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateStateServerRpc(bool state)
    {
        activated.Value = state;
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
