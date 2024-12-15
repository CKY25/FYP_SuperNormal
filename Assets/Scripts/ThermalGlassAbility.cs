using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

[CreateAssetMenu]
public class ThermalGlassAbility : Abilities
{
    public GameObject specPrefab;
    private GameObject spec = null;
    private bool isActive = false;
    private string player = "Player";
    private string invisible = "Invisible";
    private string thermalGlass = "ThermalGlass";
    public Material thermalMaterial; 
    private Dictionary<SkinnedMeshRenderer, Material> originalMaterials = new Dictionary<SkinnedMeshRenderer, Material>();
    private static GameObject playerGO;

    private void Awake()
    {
        isActive = false;
    }

    public void AbilityButtonPressedXR(InputAction.CallbackContext context)
    {
        if (playerGO != null && playerGO.GetComponent<PlayerManager>().GetAbilities().name.Equals(thermalGlass) && playerGO.GetComponent<AbilityHolder>().cdTime <= 0)
        {
            Camera playerCamera = playerGO.GetComponent<PlayerController>().playerCamera;
            if (context.performed)
            {
                isActive = true;
                if (isActive)
                {
                    if (spec == null)
                    {
                        spec = Instantiate(specPrefab, playerGO.transform.GetChild(4).GetChild(10));
                        spec.transform.localPosition = Vector3.zero;
                    }
                    ShowInvisiblePlayer(playerCamera, playerGO);
                    playerGO.GetComponent<AbilityHolder>().activeStateWait(activeTime);
                }
            }
        }
    }

    public override void Activate(GameObject parent)
    {
        if (playerGO == null) 
            playerGO = parent;
        Camera playerCamera = parent.GetComponent<PlayerController>().playerCamera;

        if (Input.GetKeyDown(KeyCode.E))
        {
            isActive = true;
        }
        
        if (isActive)
        {
            if (spec == null)
            {
                spec = Instantiate(specPrefab, parent.transform.GetChild(4).GetChild(10));
                spec.transform.localPosition = Vector3.zero;
            }
            ShowInvisiblePlayer(playerCamera, parent);
            parent.GetComponent<AbilityHolder>().activeStateWait(activeTime);
        }
    }

    public override void CoolDown(GameObject parent)
    {
        Camera playerCamera = parent.GetComponent<PlayerController>().playerCamera;
        isActive = false;
        Destroy(spec);
        ResetVisibility(playerCamera, parent);
    }

    private void ShowInvisiblePlayer(Camera playerCamera, GameObject parent)
    {
        Collider[] hitColliders = Physics.OverlapSphere(playerCamera.transform.position, 50f);
        foreach (var hitCollider in hitColliders)
        {
            GameObject hitObj = hitCollider.gameObject;
            if (hitObj.tag.Equals(player))
            {
                var playerManager = hitObj.GetComponent<PlayerManager>();
                if (playerManager != null && playerManager.selfAbility.Value.name.Equals(invisible) && !hitObj.transform.GetChild(4).gameObject.activeSelf)
                {
                    if (IsInView(playerCamera, hitCollider))
                    {
                        foreach (SkinnedMeshRenderer skinnedMeshRenderer in hitObj.transform.GetChild(4).GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            if (!originalMaterials.ContainsKey(skinnedMeshRenderer))
                            {
                                originalMaterials[skinnedMeshRenderer] = skinnedMeshRenderer.material;
                                skinnedMeshRenderer.material = thermalMaterial;
                            }
                        }
                        hitObj.transform.GetChild(4).gameObject.SetActive(true);
                    }
                }
            }
        }
    }

    private void ResetVisibility(Camera playerCamera, GameObject parent)
    {
        Collider[] hitColliders = Physics.OverlapSphere(playerCamera.transform.position, 50f);
        foreach (var hitCollider in hitColliders)
        {
            GameObject hitObj = hitCollider.gameObject;
            if (hitObj.tag.Equals(player))
            {
                var playerManager = hitObj.GetComponent<PlayerManager>();
                if (playerManager != null && playerManager.selfAbility.Value.name.Equals(invisible) && hitObj.transform.GetChild(4).gameObject.activeSelf)
                {
                    foreach (SkinnedMeshRenderer skinnedMeshRenderer in hitObj.transform.GetChild(4).GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        if (originalMaterials.ContainsKey(skinnedMeshRenderer))
                        {
                            skinnedMeshRenderer.material = originalMaterials[skinnedMeshRenderer];
                            originalMaterials.Remove(skinnedMeshRenderer);
                        }
                    }

                    NetworkObject targetNetworkObject = hitObj.GetComponent<NetworkObject>();
                    if (targetNetworkObject != null)
                    {
                        parent.GetComponent<AbilityHolder>().CheckVisibilityServerRpc(targetNetworkObject.NetworkObjectId);
                    }
                }
            }
        }
    }

    private bool IsInView(Camera cam, Collider col) 
    { 
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam); 
        return GeometryUtility.TestPlanesAABB(planes, col.bounds);
    }
}
