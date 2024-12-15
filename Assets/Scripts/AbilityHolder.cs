using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using static UnityEngine.GraphicsBuffer;

public class AbilityHolder : NetworkBehaviour
{
    public Abilities[] ability;
    public GameObject cctvPrefab;
    private GameObject realcctv;
    public float cdTime;
    float activeTime;
    private string superStrength = "SuperStrength";
    private string invisible = "Invisible";
    private string flight = "Flight";
    private string magnetism = "Magnetism";
    private string surveillance = "Surveillance";
    private string taser = "Taser";
    private string thermalGlass = "ThermalGlass";
    private string luope = "Luope";
    private string isWalking = "isWalking";
    private string vertical = "Vertical";
    private string horizontal = "Horizontal";
    private PlayerManager playerManager;
    private bool isAbilityActive = false;
    private bool EPressed = false;

    enum AbilityState
    {
        ready,
        active,
        cooldown
    }

    AbilityState state = AbilityState.ready;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
        activeTime = playerManager.GetAbilities().activeTime;
        cdTime = playerManager.GetAbilities().cdTime;

        if (IsOwner)
        {
            if (playerManager.GetAbilities().name.Equals(superStrength))
                transform.GetChild(1).GetChild(2).GetChild(0).gameObject.SetActive(true);
            else if (playerManager.GetAbilities().name.Equals(invisible))
                transform.GetChild(1).GetChild(2).GetChild(1).gameObject.SetActive(true);
            else if (playerManager.GetAbilities().name.Equals(flight))
                transform.GetChild(1).GetChild(2).GetChild(2).gameObject.SetActive(true);
            else if (playerManager.GetAbilities().name.Equals(magnetism))
                transform.GetChild(1).GetChild(2).GetChild(3).gameObject.SetActive(true);
            else if (playerManager.GetAbilities().name.Equals(surveillance))
            {
                transform.GetChild(1).GetChild(2).GetChild(4).gameObject.SetActive(true);
                if (isPresent())
                    transform.GetChild(1).GetChild(2).GetChild(14).gameObject.SetActive(true);
                else
                    transform.GetChild(1).GetChild(2).GetChild(13).gameObject.SetActive(true);
            }
            else if (playerManager.GetAbilities().name.Equals(taser))
            {
                transform.GetChild(1).GetChild(2).GetChild(5).gameObject.SetActive(true);
                if (isPresent())
                    transform.GetChild(1).GetChild(2).GetChild(14).gameObject.SetActive(true);
                else
                    transform.GetChild(1).GetChild(2).GetChild(13).gameObject.SetActive(true);
            }
            else if (playerManager.GetAbilities().name.Equals(thermalGlass))
            {
                transform.GetChild(1).GetChild(2).GetChild(6).gameObject.SetActive(true);
                if (isPresent())
                    transform.GetChild(1).GetChild(2).GetChild(14).gameObject.SetActive(true);
                else
                    transform.GetChild(1).GetChild(2).GetChild(13).gameObject.SetActive(true);
            }
            else if (playerManager.GetAbilities().name.Equals(luope))
            {
                transform.GetChild(1).GetChild(2).GetChild(7).gameObject.SetActive(true);
                playerManager.GetAbilities().ResetLuope();

                if (isPresent())
                    transform.GetChild(1).GetChild(2).GetChild(14).gameObject.SetActive(true);
                else
                    transform.GetChild(1).GetChild(2).GetChild(13).gameObject.SetActive(true);
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        switch (state)
        {
            case AbilityState.ready:
                if(!isAbilityActive)
                {
                    handleAbilityActivation();
                }
                break;
            case AbilityState.active:
                handleAbilityActive();
                break;
            case AbilityState.cooldown:
                handleAbilityCoolDown();
                break;
        }
    }

    public void AbilityButtonPressedXR(InputAction.CallbackContext context)
    {
        if (context.performed && !EPressed)
            EPressed = true;
        else if (context.canceled && EPressed)
            EPressed = false;
    }

    private void handleAbilityActivation()
    {
        if (playerManager.GetAbilities().name.Equals(invisible) && IsOwner)
        {
            if (cdTime > 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().SetText(cdTime.ToString("0"));
            }

            cdTime -= Time.deltaTime;
            if (!isPresent())
            {
                if (GetComponent<PlayerController>().curSpeedX != 0 || GetComponent<PlayerController>().curSpeedY != 0)
                {
                    cdTime = playerManager.GetAbilities().cdTime;
                }
            }
            else
            {
                if (transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed * Input.GetAxis(vertical) != 0 || transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed * Input.GetAxis(horizontal) != 0)
                {
                    cdTime = playerManager.GetAbilities().cdTime;
                }
            }

            if (cdTime <= 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().text = null;
                playerManager.GetAbilities().Activate(gameObject);
                state = AbilityState.active;
                isAbilityActive = true;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(superStrength) && IsOwner)
        {
            if (cdTime > 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().SetText(cdTime.ToString("0"));
            }

            cdTime -= Time.deltaTime;
            if(cdTime <= 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().text = null;
                playerManager.GetAbilities().Activate(gameObject);
                state = AbilityState.active;
                isAbilityActive = true;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(flight) && IsOwner)
        {
            if (cdTime > 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().SetText(cdTime.ToString("0"));
            }

            cdTime -= Time.deltaTime;
            if (cdTime <= 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().text = null;
                playerManager.GetAbilities().Activate(gameObject);
                state = AbilityState.active;
                isAbilityActive = true;
            }

            if (Input.GetKeyDown(KeyCode.E) || EPressed)
            {
                cdTime = playerManager.GetAbilities().cdTime;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(magnetism) && IsOwner)
        {
            if (cdTime > 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().SetText(cdTime.ToString("0"));
            }

            cdTime -= Time.deltaTime;
            if(cdTime <= 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().text = null;
                playerManager.GetAbilities().Activate(gameObject);
                state = AbilityState.active;
                isAbilityActive = true;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(surveillance) && IsOwner)
        {
            cdTime = 0;
            playerManager.GetAbilities().Activate(gameObject);
        }
        else if (playerManager.GetAbilities().name.Equals(taser) && IsOwner)
        {
            cdTime = 0;
            playerManager.GetAbilities().Activate(gameObject);
        }
        else if (playerManager.GetAbilities().name.Equals(thermalGlass) && IsOwner)
        {   
            cdTime = 0;
            playerManager.GetAbilities().Activate(gameObject);
        }
        else if (playerManager.GetAbilities().name.Equals(luope) && IsOwner)
        { 
            cdTime = 0;
            playerManager.GetAbilities().Activate(gameObject);
        }
    }

    private void handleAbilityActive()
    {
        if (playerManager.GetAbilities().name.Equals(invisible))
        {
            if (!isPresent())
            {
                if (gameObject.GetComponent<PlayerController>().curSpeedX > 0 || gameObject.GetComponent<PlayerController>().curSpeedY > 0)
                {
                    if (IsOwner)
                    {
                        playerManager.GetAbilities().CoolDown(gameObject);
                        state = AbilityState.ready;
                        cdTime = playerManager.GetAbilities().cdTime;
                        isAbilityActive = false;
                    }
                }
            }
            else
            {
                if (transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed * Input.GetAxis(vertical) > 0 || transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed * Input.GetAxis(horizontal) > 0)
                {
                    if (IsOwner)
                    {
                        playerManager.GetAbilities().CoolDown(gameObject);
                        state = AbilityState.ready;
                        cdTime = playerManager.GetAbilities().cdTime;
                        isAbilityActive = false;
                    }
                }
            }
        }
        else if (playerManager.GetAbilities().name.Equals(superStrength) && IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.E) || EPressed)
            {
                playerManager.GetAbilities().CoolDown(gameObject);
                state = AbilityState.ready;
                cdTime = playerManager.GetAbilities().cdTime;
                isAbilityActive = false;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(flight) && IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.E) || EPressed)
            {
                playerManager.GetAbilities().CoolDown(gameObject);
                state = AbilityState.ready;
                cdTime = playerManager.GetAbilities().cdTime;
                isAbilityActive = false;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(magnetism) && IsOwner)
        {
            playerManager.GetAbilities().CoolDown(gameObject);
            state = AbilityState.ready;
            cdTime = playerManager.GetAbilities().cdTime;
            isAbilityActive = false;
        }
        else if (playerManager.GetAbilities().name.Equals(surveillance) && IsOwner)
        {
            playerManager.GetAbilities().CoolDown(gameObject);
            state = AbilityState.cooldown;
            cdTime = playerManager.GetAbilities().cdTime;
            isAbilityActive = false;
        }
        else if (playerManager.GetAbilities().name.Equals(taser) && IsOwner)
        {
            playerManager.GetAbilities().CoolDown(gameObject);
            state = AbilityState.cooldown;
            cdTime = playerManager.GetAbilities().cdTime;
            isAbilityActive = false;
        }
        else if (playerManager.GetAbilities().name.Equals(thermalGlass) && IsOwner)
        {
            playerManager.GetAbilities().CoolDown(gameObject);
            state = AbilityState.cooldown;
            cdTime = playerManager.GetAbilities().cdTime;
            isAbilityActive = false;
        }
        else if (playerManager.GetAbilities().name.Equals(luope) && IsOwner)
        {
            playerManager.GetAbilities().CoolDown(gameObject);
            state = AbilityState.cooldown;
            cdTime = playerManager.GetAbilities().cdTime;
            isAbilityActive = false;
        }
    }

    private void handleAbilityCoolDown()
    {
        if (playerManager.GetAbilities().name.Equals(surveillance) && IsOwner)
        {
            if (cdTime > 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().SetText(cdTime.ToString("0"));
                cdTime -= Time.deltaTime;
            }
            else
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().text = null;
                state = AbilityState.ready;
                isAbilityActive = false;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(taser) && IsOwner)
        {
            if (cdTime > 0)
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().SetText(cdTime.ToString("0"));
                cdTime -= Time.deltaTime;
            }
            else
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().text = null;
                state = AbilityState.ready;
                isAbilityActive = false;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(thermalGlass) && IsOwner)
        {
            if (cdTime > 0)
            {
                StopAllCoroutines();
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().SetText(cdTime.ToString("0"));
                cdTime -= Time.deltaTime;
            }
            else
            {
                transform.GetChild(1).GetChild(2).GetChild(8).GetComponent<TMP_Text>().text = null;
                state = AbilityState.ready;
                isAbilityActive = false;
            }
        }
        else if (playerManager.GetAbilities().name.Equals(luope) && IsOwner)
        {
            state = AbilityState.ready;
            isAbilityActive = false;
        }
    }

    [ServerRpc]
    public void SetInvisibilityServerRpc(bool invisible)
    {
        SetInvisibilityClientRpc(invisible);
    }

    [ClientRpc]
    public void SetInvisibilityClientRpc(bool invisible)
    {
        if (invisible)
        {
            gameObject.transform.GetChild(4).gameObject.SetActive(false);
        }
        else
        {
            gameObject.transform.GetChild(4).gameObject.SetActive(true);
        }
    }

    public void StunEnemy(GameObject target)
    {
        NetworkObject targetNetworkObject = target.GetComponent<NetworkObject>();
        if (targetNetworkObject != null)
        {
            StunEnemyServerRpc(targetNetworkObject.NetworkObjectId);
        }
    }

    [ServerRpc]
    private void StunEnemyServerRpc(ulong targetNetworkObjectId)
    {
        StartStunEnemyClientRpc(targetNetworkObjectId);
    }

    [ClientRpc]
    private void StartStunEnemyClientRpc(ulong targetNetworkObjectId)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.GetComponent<NavMeshAgent>().enabled = false;
        target.GetComponent<Nav>().enabled = false;
        target.GetComponent<Animator>().speed = 0;
    }

    public void StartRecoverStun(GameObject target)
    {
        NetworkObject targetNetworkObject = target.GetComponent<NetworkObject>();
        if (targetNetworkObject != null)
        {
            StartRecoverStunServerRpc(targetNetworkObject.NetworkObjectId);
        }
    }

    [ServerRpc]
    public void StartRecoverStunServerRpc(ulong targetNetworkObjectId)
    {
        StartCoroutine(RecoverStun(targetNetworkObjectId, 5));
    }

    private IEnumerator RecoverStun(ulong targetNetworkObjectId, int seconds)
    {
        yield return new WaitForSeconds(seconds);
        RecoverStunEnemyClientRpc(targetNetworkObjectId);
    }

    [ClientRpc]
    private void RecoverStunEnemyClientRpc(ulong targetNetworkObjectId)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.GetComponent<NavMeshAgent>().enabled = true;
        target.GetComponent<Nav>().enabled = true;
        target.GetComponent<Animator>().speed = 1;
    }

    public void StunPlayer(GameObject target)
    {
        NetworkObject targetNetworkObject = target.GetComponent<NetworkObject>();
        if (targetNetworkObject != null)
        {
            StunPlayerServerRpc(targetNetworkObject.NetworkObjectId);
        }
    }

    [ServerRpc]
    private void StunPlayerServerRpc(ulong targetNetworkObjectId)
    {
        StartStunPlayerClientRpc(targetNetworkObjectId);
    }

    [ClientRpc]
    private void StartStunPlayerClientRpc(ulong targetNetworkObjectId)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.transform.GetChild(4).GetComponent<Animator>().speed = 0;
        target.GetComponent<PlayerController>().enabled = false;
        target.transform.GetChild(1).GetChild(2).GetChild(15).gameObject.SetActive(true);

        if (isPresent())
        {
            target.transform.GetChild(10).gameObject.SetActive(false);
        }
    }

    public void StartRecoverStunPlayer(GameObject target)
    {
        NetworkObject targetNetworkObject = target.GetComponent<NetworkObject>();
        if (targetNetworkObject != null)
        {
            StartRecoverStunPlayerServerRpc(targetNetworkObject.NetworkObjectId);
        }
    }

    [ServerRpc]
    public void StartRecoverStunPlayerServerRpc(ulong targetNetworkObjectId)
    {
        StartCoroutine(RecoverStunPlayer(targetNetworkObjectId, 5));
    }

    private IEnumerator RecoverStunPlayer(ulong targetNetworkObjectId, int seconds)
    {
        yield return new WaitForSeconds(seconds);
        RecoverStunPlayerClientRpc(targetNetworkObjectId);
    }

    [ClientRpc]
    private void RecoverStunPlayerClientRpc(ulong targetNetworkObjectId)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.transform.GetChild(4).GetComponent<Animator>().speed = 1;
        target.GetComponent<PlayerController>().enabled = true;
        target.transform.GetChild(1).GetChild(2).GetChild(15).gameObject.SetActive(false);
        if (isPresent())
        {
            target.transform.GetChild(10).gameObject.SetActive(true);
        }
    }

    [ServerRpc]
    public  void CheckVisibilityServerRpc(ulong targetNetworkObjectId)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        if (targetNetworkObject != null)
        {
            GameObject target = targetNetworkObject.gameObject;
            if (target.transform.GetChild(4).gameObject.activeSelf)
            {
                UpdateVisibilityClientRpc(targetNetworkObjectId, true);
            }
            else
            {
                UpdateVisibilityClientRpc(targetNetworkObjectId, false);
            }
        }
    }

    [ClientRpc]
    private void UpdateVisibilityClientRpc(ulong targetNetworkObjectId, bool isVisible)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.transform.GetChild(4).gameObject.SetActive(isVisible);
    }

    [ServerRpc(RequireOwnership = false)]
    public void InstantiateCCTVServerRpc(Vector3 point, Quaternion rotation)
    {
        GameObject instantiatedCCTV = Instantiate(cctvPrefab, point, rotation);
        instantiatedCCTV.GetComponent<NetworkObject>().Spawn();
        realcctv = instantiatedCCTV.gameObject;
        UpdateRealCCTVClientRpc(instantiatedCCTV.GetComponent<NetworkObject>().NetworkObjectId);
    }

    [ClientRpc]
    private void UpdateRealCCTVClientRpc(ulong networkObjectId) 
    { 
        NetworkObject netObj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        realcctv = netObj.gameObject;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DestroyCCTVServerRpc(ulong id)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[id];
        GameObject target = targetNetworkObject.gameObject;
        target.GetComponent<NetworkObject>().Despawn();
        realcctv = null;
    }

    public GameObject getCCTVPrefab()
    {
        return realcctv;
    }

    public void activeState()
    {
        state = AbilityState.active;
        isAbilityActive = true;
    }

    public void activeStateWait(float seconds)
    {
        StartCoroutine(activeState(seconds));
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

    private IEnumerator activeState(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        state = AbilityState.active;
        isAbilityActive = true;
    }
}