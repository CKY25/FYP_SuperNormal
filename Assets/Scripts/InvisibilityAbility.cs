using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu]
public class InvisibilityAbility : Abilities
{
    public AudioClip disappearSound;

    public override void Activate(GameObject parent)
    {
        var abilityHolder = parent.GetComponent<AbilityHolder>();
        if (abilityHolder != null)
        {
            abilityHolder.SetInvisibilityServerRpc(true);
        }

        if (parent.GetComponent<AudioSource>().clip == null)
        {
            parent.GetComponent<AudioSource>().clip = disappearSound;
        }

        parent.GetComponent<AudioSource>().Play();
    }

    public override void CoolDown(GameObject parent)
    {
        var abilityHolder = parent.GetComponent<AbilityHolder>();
        if (abilityHolder != null)
        {
            abilityHolder.SetInvisibilityServerRpc(false);
        }
    }
}