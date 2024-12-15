using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu]
public class LuopeAbility : Abilities
{
    public GameObject luopePrefab;
    private GameObject luope = null;
    private string npc = "NPC";
    private string player = "Player";
    private string isNPC = "The character is an NPC";
    private string isPlayer = "The character is a Player";
    private string luopeStr = "Luope";
    private bool usedLuope;
    private bool isActive;
    private static GameObject playerGO;

    private void Awake()
    {
        usedLuope = false;
        isActive = false;
    }

    public void AbilityButtonPressedXR(InputAction.CallbackContext context)
    {
        if(playerGO != null && playerGO.GetComponent<PlayerManager>().GetAbilities().name.Equals(luopeStr))
        if (context.performed && !usedLuope)
        {
            isActive = !isActive;
        }
    }

    public void triggerPressedXR(InputAction.CallbackContext context)
    {
        if (playerGO != null && playerGO.GetComponent<PlayerManager>().GetAbilities().name.Equals(luopeStr))
        {
            Camera playerCamera = playerGO.GetComponent<PlayerController>().playerCamera;
            RaycastHit hit;

            if (isActive && !usedLuope)
            {
                if (luope == null)
                {
                    luope = Instantiate(luopePrefab, playerGO.transform.GetChild(4).GetChild(10));
                    luope.transform.localPosition = Vector3.zero;
                }
                playerGO.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (context.performed)
                    {
                        if (hit.transform.gameObject.tag.Equals(npc))
                        {
                            playerGO.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().SetText(isNPC);
                            playerGO.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                            usedLuope = true;
                            isActive = false;
                            playerGO.GetComponent<AbilityHolder>().activeStateWait(3);
                        }
                        else if (hit.transform.gameObject.tag.Equals(player))
                        {
                            playerGO.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().SetText(isPlayer);
                            playerGO.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                            usedLuope = true;
                            isActive = false;
                            playerGO.GetComponent<AbilityHolder>().activeStateWait(3);
                        }
                    }
                }
            }
            else
            {
                Destroy(luope);
                playerGO.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public override void Activate(GameObject parent)
    {
        if(playerGO == null)
            playerGO = parent;
        Camera playerCamera = parent.GetComponent<PlayerController>().playerCamera;
        RaycastHit hit;
        if (Input.GetKeyDown(KeyCode.E) && !usedLuope)
        {
            isActive = !isActive;
        }
        if (isActive && !usedLuope)
        {
            if (luope == null)
            {
                luope = Instantiate(luopePrefab, parent.transform.GetChild(4).GetChild(10));
                luope.transform.localPosition = Vector3.zero;
            }
            parent.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (hit.transform.gameObject.tag.Equals(npc))
                    {
                        parent.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().SetText(isNPC);
                        parent.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                        usedLuope = true;
                        isActive = false;
                        parent.GetComponent<AbilityHolder>().activeStateWait(3);
                    }
                    else if (hit.transform.gameObject.tag.Equals(player))
                    {
                        parent.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().SetText(isPlayer);
                        parent.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                        usedLuope = true;
                        isActive = false;
                        parent.GetComponent<AbilityHolder>().activeStateWait(3);
                    }
                }
            }
        }
        else
        {
            Destroy(luope);
            parent.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        }
    }

    public override void CoolDown(GameObject parent)
    {
        isActive = false;
        parent.transform.GetChild(1).GetChild(1).GetComponent<TMP_Text>().text = null;
        Destroy(luope);
    }

    public override void ResetLuope()
    {
        usedLuope = false;
    }
}