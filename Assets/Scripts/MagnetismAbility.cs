using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MagnetismAbility : Abilities
{
    public float magnetRange = 5f;
    public float attractionSpeed = 5f;

    public override void Activate(GameObject parent)
    {
        parent.GetComponent<PlayerController>().StartAttractingObjects(magnetRange, attractionSpeed);
    }

    public override void CoolDown(GameObject parent)
    {

    }
}
