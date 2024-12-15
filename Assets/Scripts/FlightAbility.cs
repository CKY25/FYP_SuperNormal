using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[CreateAssetMenu]
public class FlightAbility : Abilities
{
    private string isFloating = "isFloating";
    private string isWalking = "isWalking";
    public AudioClip wingFlapSound;

    public override void Activate(GameObject parent)
    {
        PlayerController controller = parent.GetComponent<PlayerController>();
        controller.gravity = 0f;
        controller.moveDirection.y = 0;
        if (parent.GetComponent<AudioSource>().clip == null)
        {
            parent.GetComponent<AudioSource>().clip = wingFlapSound;
            parent.GetComponent<AudioSource>().loop = true;
        }
            
        if(!parent.GetComponent<AudioSource>().isPlaying)
            parent.GetComponent<AudioSource>().Play();
        Animator animator = parent.transform.GetChild(4).GetComponent<Animator>();
        animator.SetBool(isWalking, false);
        animator.SetBool(isFloating, true);
        if (isPresent())
        {
            parent.transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().useGravity = false;
            parent.transform.GetChild(1).GetChild(2).GetChild(14).gameObject.SetActive(true);
        }
        else
        {
            parent.transform.GetChild(1).GetChild(2).GetChild(13).gameObject.SetActive(true);
        }
    }

    public override void CoolDown(GameObject parent)
    {
        PlayerController controller = parent.GetComponent<PlayerController>();
        controller.gravity = 20f;
        parent.GetComponent<AudioSource>().Pause();
        Animator animator = parent.transform.GetChild(4).GetComponent<Animator>();
        animator.SetBool(isFloating, false);
        if (isPresent())
        {
            parent.transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().useGravity = true;
            parent.transform.GetChild(1).GetChild(2).GetChild(14).gameObject.SetActive(false);
        }
        else
        {
            parent.transform.GetChild(1).GetChild(2).GetChild(13).gameObject.SetActive(false);
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
