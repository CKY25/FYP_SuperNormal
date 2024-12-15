using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CollectArmour : NetworkBehaviour
{
    private string metalObj = "MetalObject";
    private string player = "Player";
    private string untag = "Untagged";
    private string vest = "Vest";
    private string legArmor = "leg armor";
    private string armArmor = "Arm Armor";
    private string cuirass = "cuirass";
    private List<GameObject> attachedArmour = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(metalObj))
        {
            if (other.name.Equals(vest) || other.name.Equals(legArmor) || other.name.Equals(armArmor) || other.name.Equals(cuirass))
            {
                other.transform.parent = transform;
                other.transform.localPosition = Vector3.zero;
                other.transform.rotation = Quaternion.identity;
                other.tag = untag;
                attachedArmour.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Equals(player))
        {
            for (int i = 0; i < attachedArmour.Count; i++)
            {
                other.GetComponent<MissionManager>().metalObjectIncrement();
            }

            attachedArmour.Clear();
        }
    }
}
