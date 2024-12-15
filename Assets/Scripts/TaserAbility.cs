using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[CreateAssetMenu]
public class TaserAbility : Abilities
{
    public GameObject taserPrefab;
    public AudioClip laserSound;
    private GameObject taser = null;
    private string npc = "NPC";
    private string player = "Player";
    private string isWalking = "isWalking";
    private string taserStr = "Taser";
    private bool holdingTaser;
    private static GameObject playerGO;

    private void Awake()
    {
        holdingTaser = false;
    }

    public void AbilityButtonPressedXR(InputAction.CallbackContext context)
    {
        if (playerGO != null && playerGO.GetComponent<PlayerManager>().GetAbilities().name.Equals(taserStr) && playerGO.GetComponent<AbilityHolder>().cdTime <= 0)
        {
            if (context.performed)
            {
                if (!holdingTaser)
                {
                    playerGO.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                    taser = Instantiate(taserPrefab, playerGO.transform.GetChild(4).GetChild(10));
                    taser.transform.localPosition = Vector3.zero;
                    holdingTaser = true;
                }
                else if (holdingTaser)
                {
                    playerGO.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                    Destroy(taser);
                    holdingTaser = false;
                }
            }   
        }
    }

    public void triggerPressedXR(InputAction.CallbackContext context)
    {
        if (playerGO != null && playerGO.GetComponent<PlayerManager>().GetAbilities().name.Equals(taserStr))
        {
            Camera playerCamera = playerGO.GetComponent<PlayerController>().playerCamera;
            RaycastHit hit;
            if (holdingTaser)
            {
                if (context.performed)
                {
                    if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
                    {
                        if (hit.transform.gameObject.tag == npc)
                        {
                            playerGO.GetComponent<AudioSource>().clip = laserSound;
                            playerGO.GetComponent<AudioSource>().Play();
                            GameObject target = hit.transform.gameObject;
                            playerGO.GetComponent<AbilityHolder>().StunEnemy(target);
                            playerGO.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                            Destroy(taser);
                            holdingTaser = false;
                            playerGO.GetComponent<AbilityHolder>().StartRecoverStun(target);
                            playerGO.GetComponent<AbilityHolder>().activeState();
                        }
                        if (hit.transform.gameObject.tag == player)
                        {
                            playerGO.GetComponent<AudioSource>().clip = laserSound;
                            playerGO.GetComponent<AudioSource>().Play();
                            GameObject target = hit.transform.gameObject;
                            playerGO.GetComponent<AbilityHolder>().StunPlayer(target);
                            playerGO.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                            Destroy(taser);
                            holdingTaser = false;
                            playerGO.GetComponent<AbilityHolder>().StartRecoverStunPlayer(target);
                            playerGO.GetComponent<AbilityHolder>().activeState();
                        }
                    }
                }
            }
        }
    }

    public override void Activate(GameObject parent)
    {
        if(playerGO == null)
            playerGO = parent;

        Camera playerCamera = parent.GetComponent<PlayerController>().playerCamera;
        RaycastHit hit;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!holdingTaser)
            {
                parent.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
                taser = Instantiate(taserPrefab, parent.transform.GetChild(4).GetChild(10));
                taser.transform.localPosition = Vector3.zero;
                holdingTaser = true;
            }
            else if (holdingTaser)
            {
                parent.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                Destroy(taser);
                holdingTaser = false;
            }
        }
        if (holdingTaser)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform.gameObject.tag == npc)
                    {
                        parent.GetComponent<AudioSource>().clip = laserSound;
                        parent.GetComponent<AudioSource>().Play();
                        GameObject target = hit.transform.gameObject;
                        parent.GetComponent<AbilityHolder>().StunEnemy(target);
                        parent.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                        Destroy(taser);
                        holdingTaser = false;
                        parent.GetComponent<AbilityHolder>().StartRecoverStun(target);
                        parent.GetComponent<AbilityHolder>().activeState();
                    }
                    if (hit.transform.gameObject.tag == player)
                    {
                        parent.GetComponent<AudioSource>().clip = laserSound;
                        parent.GetComponent<AudioSource>().Play();
                        GameObject target = hit.transform.gameObject;
                        parent.GetComponent<AbilityHolder>().StunPlayer(target);
                        parent.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
                        Destroy(taser);
                        holdingTaser = false;
                        parent.GetComponent<AbilityHolder>().StartRecoverStunPlayer(target);
                        parent.GetComponent<AbilityHolder>().activeState();
                    }
                }
            }
        }
    }
    public override void CoolDown(GameObject parent) {  
        // cool down logic                                                
    } 
}