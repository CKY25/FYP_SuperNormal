using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PassCup : NetworkBehaviour
{
    [SerializeField] private GameObject intactMug;
    [SerializeField] private GameObject brokenMug;
    [SerializeField] private GameObject cupPrefab;
    private string player = "Player";
    private string npc = "NPC";
    private string superStrength = "SuperStrength";
    private string msg = "Press F to Take Cup";
    private string msg2 = "Press F to Retry";
    private string msgXR = "Press Left Trigger to Take Cup";
    private string msgXR2 = "Press Left Trigger to Retry";
    private string isDrinking = "isDrinking";
    private string vertical = "Vertical";
    private string horizontal = "Horizontal";
    private GameObject playerCharacter;
    private GameObject nonPlayerCharacter;
    private bool isSuperStrength = false;
    private bool isHoldingMug = false;
    private bool isBroken = false;
    private bool isActivated = false;

    private void Awake()
    {
        intactMug.SetActive(true);
        brokenMug.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(player) && other.GetComponent<PlayerController>().IsLocalPlayer && other.GetComponent<PlayerManager>().GetAbilities().name.Equals(superStrength) && !isBroken)
        {
            playerCharacter = other.gameObject;
            other.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(true);

            if(isPresent())
                other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msgXR;
            else
                other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msg;
            isSuperStrength = true;
        }
        else if (other.gameObject.tag.Equals(player) && other.GetComponent<PlayerController>().IsLocalPlayer && other.GetComponent<PlayerManager>().GetAbilities().name.Equals(superStrength) && isBroken)
        {
            playerCharacter = other.gameObject;
            other.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(true);
            if(isPresent())
                other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msgXR2;
            else
                other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = msg2;
            isSuperStrength = true;
        }

        if (other.gameObject.tag.Equals(npc) && isHoldingMug && !isBroken)
        {
            nonPlayerCharacter = other.gameObject;
            passCupNPCServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, other.GetComponent<NetworkObject>().NetworkObjectId);
            UpdateActivatedServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId, true);
            UpdateNPCAimatorServerRpc(other.GetComponent<NetworkObject>().NetworkObjectId, isDrinking, true);
            isHoldingMug = false;
            StartCoroutine(Wait(other.GetComponent<NetworkObject>().NetworkObjectId));
            playerCharacter.GetComponent<MissionManager>().passWaterIncrement();
        }
    }

    public void PassWaterTriggerPressedXR(InputAction.CallbackContext context)
    {
        if (isSuperStrength)
        {
            if (context.performed && !isHoldingMug)
            {
                playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(false);
                playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
                takeCupServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, playerCharacter.GetComponent<NetworkObject>().NetworkObjectId);
                isHoldingMug = true;
            }
            else if (context.performed && isHoldingMug)
            {
                putCupServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId);
                isHoldingMug = false;
            }

            if (isBroken)
            {
                if (context.performed)
                {
                    instantiateCupServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId);
                    isHoldingMug = false;
                    isBroken = false;
                    playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(false);
                    playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
                }
            }
        }
    }

    private void Update()
    {
        if (isSuperStrength)
        {
            if (Input.GetKeyDown(KeyCode.F) && !isHoldingMug)
            {
                playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(false);
                playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
                takeCupServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, playerCharacter.GetComponent<NetworkObject>().NetworkObjectId);
                isHoldingMug = true;
            }
            else if (Input.GetKeyDown(KeyCode.F) && isHoldingMug)
            {
                putCupServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId);
                isHoldingMug = false;
            }

            if (isBroken)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    instantiateCupServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId);
                    isHoldingMug = false;
                    isBroken = false;
                    playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(false);
                    playerCharacter.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
                }
            }
        }

        if (isHoldingMug)
        {
            float speedX;
            float speedY;
            if (!isPresent())
            {
                speedX = playerCharacter.GetComponent<PlayerController>().curSpeedX;
                speedY = playerCharacter.GetComponent<PlayerController>().curSpeedY;
            }
            else
            {
                speedX = playerCharacter.transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed * Input.GetAxis(vertical);
                speedY = playerCharacter.transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed * Input.GetAxis(horizontal);
            }
            
            if (speedX > 5 || speedX < -5 || speedY > 5 || speedY < -5)
            {
                BreakCupServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId);
            }
        }

        if (isActivated && nonPlayerCharacter != null)
        {
            DrinkWaterServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId, nonPlayerCharacter.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals(player) && other.GetComponent<PlayerController>().IsLocalPlayer && other.GetComponent<PlayerManager>().GetAbilities().name.Equals(superStrength))
        {
            other.transform.GetChild(1).GetChild(2).GetChild(12).gameObject.SetActive(false);
            other.transform.GetChild(1).GetChild(2).GetChild(12).GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
            isSuperStrength = false;
            if (other.gameObject == playerCharacter && !isHoldingMug)
            {
                playerCharacter = null;
            }
        }
    }

    [ServerRpc (RequireOwnership = false)]
    private void BreakCupServerRpc(ulong glassObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        BreakCupClientRpc(glassObjectId);
        glassObj.transform.SetParent(null);
    }

    [ClientRpc]
    private void BreakCupClientRpc(ulong glassObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;

        glassObj.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;
        glassObj.transform.GetChild(0).gameObject.SetActive(false);
        glassObj.transform.GetChild(1).gameObject.SetActive(true);

        for (int i = 0; i < glassObj.transform.GetChild(1).transform.childCount; i++) 
        {
            glassObj.transform.GetChild(1).GetChild(i).GetComponent<Rigidbody>().AddExplosionForce(10f, glassObj.transform.GetChild(1).GetChild(i).position, 2f);
        }

        isHoldingMug = false;
        isBroken = true;
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
        target.GetComponent<NavMeshAgent>().isStopped = true;
        target.GetComponent<NavMeshAgent>().velocity = Vector3.zero;
        target.GetComponent<NavMeshAgent>().enabled = false;
        target.GetComponent<Nav>().enabled = false;
        isActivated = state;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateDeactivatedServerRpc(ulong targetNetworkObjectId, bool state)
    {
        UpdateDeactivatedClientRpc(targetNetworkObjectId, state);
    }

    [ClientRpc]
    private void UpdateDeactivatedClientRpc(ulong targetNetworkObjectId, bool state)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.GetComponent<NavMeshAgent>().enabled = true;
        target.GetComponent<NavMeshAgent>().isStopped = false;   
        target.GetComponent<Nav>().enabled = true;
        isActivated = state;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateNPCAimatorServerRpc(ulong targetNetworkObjectId, string animationName, bool state)
    {
        UpdateNPCAimatorClientRpc(targetNetworkObjectId, animationName, state);
    }

    [ClientRpc]
    private void UpdateNPCAimatorClientRpc(ulong targetNetworkObjectId, string animationName, bool state)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.GetComponent<Animator>().SetBool(animationName, state);
    }

    [ServerRpc(RequireOwnership = false)]
    private void takeCupServerRpc(ulong glassObjectId, ulong playerObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        NetworkObject playerCharacter = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerObjectId];
        GameObject playerObj = playerCharacter.gameObject;

        glassObj.transform.SetParent(playerObj.transform);
        glassObj.transform.localPosition = new Vector3(0.273f, 2.10f, 0.719f);
        glassObj.transform.GetChild(0).localPosition = new Vector3(0, 0.1f, 0);
        glassObj.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = true;
        takeCupClientRpc(glassObjectId, playerObjectId);
    }

    [ClientRpc]
    private void takeCupClientRpc(ulong glassObjectId, ulong playerObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        NetworkObject playerCharacter = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerObjectId];
        GameObject playerObj = playerCharacter.gameObject;

        glassObj.transform.localPosition = new Vector3(0.273f, 2.10f, 0.719f);
        glassObj.transform.GetChild(0).localPosition = new Vector3(0, 0.1f, 0);
        glassObj.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void putCupServerRpc(ulong glassObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        glassObj.transform.SetParent(null);
        glassObj.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;
        putCupClientRpc(glassObjectId);
    }

    [ClientRpc]
    private void putCupClientRpc(ulong glassObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        glassObj.transform.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void instantiateCupServerRpc(ulong glassObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        Destroy(glassObj);

        var newmug = Instantiate(cupPrefab);
        newmug.transform.position = new Vector3(16.47f, -1.1f, -2.017f);
        newmug.GetComponent<PassCup>().enabled = true;
        newmug.GetComponent<NetworkObject>().enabled = true;
        newmug.GetComponent<ClientNetworkTransform>().enabled = true;
        newmug.GetComponent<NetworkObject>().Spawn();
        newmug.GetComponent<PlayerInput>().enabled = true;
    }

    [ServerRpc (RequireOwnership = false)]
    private void passCupNPCServerRpc(ulong glassObjectId, ulong nonPlayerObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        NetworkObject nonPlayerCharacter = NetworkManager.Singleton.SpawnManager.SpawnedObjects[nonPlayerObjectId];
        GameObject nonPlayerObj = nonPlayerCharacter.gameObject;
        passCupNPCClientRpc(glassObjectId, nonPlayerObjectId);
        glassObj.transform.SetParent(nonPlayerObj.gameObject.transform);
    }

    [ClientRpc]
    private void passCupNPCClientRpc(ulong glassObjectId, ulong nonPlayerObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        NetworkObject nonPlayerCharacter = NetworkManager.Singleton.SpawnManager.SpawnedObjects[nonPlayerObjectId];
        GameObject nonPlayerObj = nonPlayerCharacter.gameObject;
        glassObj.transform.localPosition = nonPlayerObj.gameObject.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1).position;
    }


    [ServerRpc(RequireOwnership = false)]
    private void DrinkWaterServerRpc(ulong glassObjectId, ulong nonPlayerObjectId)
    {
        DrinkWaterClientRpc(glassObjectId, nonPlayerObjectId);
    }

    [ClientRpc]
    private void DrinkWaterClientRpc(ulong glassObjectId, ulong nonPlayerObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        NetworkObject nonPlayerCharacter = NetworkManager.Singleton.SpawnManager.SpawnedObjects[nonPlayerObjectId];
        GameObject nonPlayerObj = nonPlayerCharacter.gameObject;

        Transform handPosition = nonPlayerObj.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(1);

        glassObj.transform.position = handPosition.position;
        glassObj.transform.rotation = handPosition.rotation;
    }

    private IEnumerator Wait(ulong targetNetworkObjectId)
    {
        yield return new WaitForSeconds(9f);

        UpdateDeactivatedServerRpc(targetNetworkObjectId, false);
        UpdateNPCAimatorServerRpc(targetNetworkObjectId, isDrinking, false);
        nonPlayerCharacter = null;
        destroyCupServerRpc(this.gameObject.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void destroyCupServerRpc(ulong glassObjectId)
    {
        NetworkObject glass = NetworkManager.Singleton.SpawnManager.SpawnedObjects[glassObjectId];
        GameObject glassObj = glass.gameObject;
        glassObj.GetComponent<NetworkObject>().Despawn();
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
