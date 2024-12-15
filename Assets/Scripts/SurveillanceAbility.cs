using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[CreateAssetMenu]
public class SurveillanceAbility : Abilities
{
    public GameObject cctvPrefab;
    private GameObject virtualCCTV = null; // To hold the virtual CCTV
    private GameObject realcctv;
    private bool isVirtualCCTVActive = false; // Track if virtual CCTV is active
    private bool insideCCTV = false;
    private float time = 0;
    private string surveillance = "Surveillance";
    private static GameObject playerGO;

    public void AbilityButtonPressedXR(InputAction.CallbackContext context)
    {
        if (playerGO != null && playerGO.GetComponent<PlayerManager>().GetAbilities().name.Equals(surveillance) && playerGO.GetComponent<AbilityHolder>().cdTime <= 0)
        {
            Camera playerCamera = playerGO.GetComponent<PlayerController>().playerCamera;
            RaycastHit hit;
            realcctv = playerGO.GetComponent<AbilityHolder>().getCCTVPrefab();
            if (realcctv == null)
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, 10, ~0, QueryTriggerInteraction.Ignore))
                {
                    // Create virtual CCTV when E is pressed
                    if (!isVirtualCCTVActive)
                    {
                        if (context.performed)
                        {
                            if (virtualCCTV != null)
                            {
                                Destroy(virtualCCTV); // Remove the previous virtual CCTV
                            }
                            virtualCCTV = Instantiate(cctvPrefab, hit.point, Quaternion.identity); // Create virtual CCTV
                            isVirtualCCTVActive = true; // Mark as active
                        }
                    }
                    else if (isVirtualCCTVActive)
                    {
                        virtualCCTV.transform.position = hit.point; // Keep it at the hit point
                        virtualCCTV.transform.LookAt(hit.point + playerCamera.transform.forward); // Face the camera direction

                        if (context.performed)
                        {
                            Destroy(virtualCCTV);
                            isVirtualCCTVActive = false;
                        }
                    }
                }
            }
            else
            {
                if (!insideCCTV)
                {
                    if (context.performed)
                    {
                        if (!isPresent())
                            playerGO.GetComponent<PlayerController>().enabled = false;
                        else
                        {
                            playerGO.transform.GetChild(10).gameObject.SetActive(false);
                        }

                        playerGO.GetComponent<EliminateSuperNormies>().enabled = false;
                        realcctv.transform.GetChild(3).gameObject.SetActive(true);
                        insideCCTV = !insideCCTV;

                        playerGO.transform.GetChild(4).GetChild(1).gameObject.SetActive(true);
                        playerGO.transform.GetChild(4).GetChild(2).gameObject.SetActive(true);
                        playerGO.transform.GetChild(4).GetChild(3).gameObject.SetActive(true);
                        playerGO.transform.GetChild(4).GetChild(4).gameObject.SetActive(true);
                        playerGO.transform.GetChild(4).GetChild(5).gameObject.SetActive(true);
                        playerGO.transform.GetChild(4).GetChild(6).gameObject.SetActive(true);
                        playerGO.transform.GetChild(4).GetChild(7).gameObject.SetActive(true);
                        playerGO.transform.GetChild(4).GetChild(8).gameObject.SetActive(true);
                        playerGO.transform.GetChild(4).GetChild(9).gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (context.performed)
                    {
                        realcctv.transform.GetChild(3).gameObject.SetActive(false);
                        if (!isPresent())
                            playerGO.GetComponent<PlayerController>().enabled = true;
                        else
                        {
                            playerGO.transform.GetChild(10).gameObject.SetActive(true);
                        }

                        playerGO.GetComponent<EliminateSuperNormies>().enabled = true;
                        insideCCTV = !insideCCTV;
                        playerGO.GetComponent<AbilityHolder>().activeState();

                        playerGO.transform.GetChild(4).GetChild(1).gameObject.SetActive(false);
                        playerGO.transform.GetChild(4).GetChild(2).gameObject.SetActive(false);
                        playerGO.transform.GetChild(4).GetChild(3).gameObject.SetActive(false);
                        playerGO.transform.GetChild(4).GetChild(4).gameObject.SetActive(false);
                        playerGO.transform.GetChild(4).GetChild(5).gameObject.SetActive(false);
                        playerGO.transform.GetChild(4).GetChild(6).gameObject.SetActive(false);
                        playerGO.transform.GetChild(4).GetChild(7).gameObject.SetActive(false);
                        playerGO.transform.GetChild(4).GetChild(8).gameObject.SetActive(false);
                        playerGO.transform.GetChild(4).GetChild(9).gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public void triggerPressedXR(InputAction.CallbackContext context)
    {
        if (playerGO != null && playerGO.GetComponent<PlayerManager>().GetAbilities().name.Equals(surveillance))
        {
            Camera playerCamera = playerGO.GetComponent<PlayerController>().playerCamera;
            RaycastHit hit;
            realcctv = playerGO.GetComponent<AbilityHolder>().getCCTVPrefab();
            if (realcctv == null)
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, 10, ~0, QueryTriggerInteraction.Ignore))
                {
                    // Spawn actual CCTV on left mouse click
                    if (context.performed)
                    {
                        playerGO.GetComponent<AbilityHolder>().InstantiateCCTVServerRpc(hit.point, virtualCCTV.transform.rotation);
                        realcctv = playerGO.GetComponent<AbilityHolder>().getCCTVPrefab();
                        Destroy(virtualCCTV); // Remove virtual CCTV
                        isVirtualCCTVActive = false; // Reset active state
                    }
                }
            }
        }
    }

    public void secondaryPressedXR(InputAction.CallbackContext context)
    {
        if (realcctv != null)
        {
            if (!insideCCTV)
            {
                if (context.performed)
                {
                    playerGO.GetComponent<AbilityHolder>().DestroyCCTVServerRpc(realcctv.GetComponent<NetworkObject>().NetworkObjectId);
                    realcctv = null;
                }
            }
        }
    }

    public override void Activate(GameObject parent)
    {
        playerGO = parent;
        Camera playerCamera = parent.GetComponent<PlayerController>().playerCamera;
        RaycastHit hit;
        realcctv = parent.GetComponent<AbilityHolder>().getCCTVPrefab();
        if(realcctv == null)
        {
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.TransformDirection(Vector3.forward), out hit, 10, ~0, QueryTriggerInteraction.Ignore))
            {
                // Create virtual CCTV when E is pressed
                if(!isVirtualCCTVActive)
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        if (virtualCCTV != null)
                        {
                            Destroy(virtualCCTV); // Remove the previous virtual CCTV
                        }
                        virtualCCTV = Instantiate(cctvPrefab, hit.point, Quaternion.identity); // Create virtual CCTV
                        isVirtualCCTVActive = true; // Mark as active
                    }
                }
                else if (isVirtualCCTVActive)
                {
                    virtualCCTV.transform.position = hit.point; // Keep it at the hit point
                    virtualCCTV.transform.LookAt(hit.point + playerCamera.transform.forward); // Face the camera direction

                    if(Input.GetKeyDown(KeyCode.E))
                    {
                        Destroy(virtualCCTV);
                        isVirtualCCTVActive = false;
                    }
                }

                // Spawn actual CCTV on left mouse click
                if (isVirtualCCTVActive && Input.GetKeyDown(KeyCode.Mouse0))
                {
                    parent.GetComponent<AbilityHolder>().InstantiateCCTVServerRpc(hit.point, virtualCCTV.transform.rotation);
                    realcctv = parent.GetComponent<AbilityHolder>().getCCTVPrefab();
                    Destroy(virtualCCTV); // Remove virtual CCTV
                    isVirtualCCTVActive = false; // Reset active state
                }
            }
        }
        else
        {
            if(!insideCCTV)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (!isPresent())
                        parent.GetComponent<PlayerController>().enabled = false;
                    else
                    {
                        parent.transform.GetChild(10).gameObject.SetActive(false);
                    }
                    parent.GetComponent<EliminateSuperNormies>().enabled = false;
                    realcctv.transform.GetChild(3).gameObject.SetActive(true);
                    insideCCTV = !insideCCTV;

                    parent.transform.GetChild(4).GetChild(1).gameObject.SetActive(true);
                    parent.transform.GetChild(4).GetChild(2).gameObject.SetActive(true);
                    parent.transform.GetChild(4).GetChild(3).gameObject.SetActive(true);
                    parent.transform.GetChild(4).GetChild(4).gameObject.SetActive(true);
                    parent.transform.GetChild(4).GetChild(5).gameObject.SetActive(true);
                    parent.transform.GetChild(4).GetChild(6).gameObject.SetActive(true);
                    parent.transform.GetChild(4).GetChild(7).gameObject.SetActive(true);
                    parent.transform.GetChild(4).GetChild(8).gameObject.SetActive(true);
                    parent.transform.GetChild(4).GetChild(9).gameObject.SetActive(true);
                }
                else if (Input.GetKey(KeyCode.F))
                {
                    time += Time.deltaTime;
                }
                else if (Input.GetKeyUp(KeyCode.F))
                {
                    time = 0;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    realcctv.transform.GetChild(3).gameObject.SetActive(false);
                    if(!isPresent())
                        parent.GetComponent<PlayerController>().enabled = true;
                    else
                    {
                        parent.transform.GetChild(10).gameObject.SetActive(true);
                    }

                    parent.GetComponent<EliminateSuperNormies>().enabled = true;
                    insideCCTV = !insideCCTV;
                    parent.GetComponent<AbilityHolder>().activeState();

                    parent.transform.GetChild(4).GetChild(1).gameObject.SetActive(false);
                    parent.transform.GetChild(4).GetChild(2).gameObject.SetActive(false);
                    parent.transform.GetChild(4).GetChild(3).gameObject.SetActive(false);
                    parent.transform.GetChild(4).GetChild(4).gameObject.SetActive(false);
                    parent.transform.GetChild(4).GetChild(5).gameObject.SetActive(false);
                    parent.transform.GetChild(4).GetChild(6).gameObject.SetActive(false);
                    parent.transform.GetChild(4).GetChild(7).gameObject.SetActive(false);
                    parent.transform.GetChild(4).GetChild(8).gameObject.SetActive(false);
                    parent.transform.GetChild(4).GetChild(9).gameObject.SetActive(false);
                }
            }

            if (time >= 3)
            {
                parent.GetComponent<AbilityHolder>().DestroyCCTVServerRpc(realcctv.GetComponent<NetworkObject>().NetworkObjectId);
                realcctv = null;
                time = 0;
            }
        }
        
    }

    public override void CoolDown(GameObject parent)
    {
        // cool down logic
        
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
