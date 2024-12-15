using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[CreateAssetMenu]
public class SuperStrengthAbility : Abilities
{
    public override void Activate(GameObject parent)
    {
        if (!isPresent())
        {
            PlayerController controller = parent.GetComponent<PlayerController>();
            controller.walkingSpeed = 10f;
            //parent.transform.GetChild(4).GetComponent<Animator>().speed = 2.86f;
        }
        else
        {
            parent.transform.GetChild(10).GetChild(1).GetComponent<DynamicMoveProvider>().moveSpeed = 10f;
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
