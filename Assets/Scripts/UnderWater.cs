using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UnderWater : NetworkBehaviour
{
    private string player = "Player";
    private string isWalking = "isWalking";
    private string isThreading = "isThreading";

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals(player))
        {
            other.GetComponent<PlayerController>().isUnderWater = true;
            other.transform.GetChild(4).GetComponent<Animator>().SetBool(isWalking, false);
            other.transform.GetChild(4).GetComponent<Animator>().SetBool(isThreading, true);
            gameObject.GetComponent<AudioSource>().Play();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals(player))
        {
            other.GetComponent<PlayerController>().isUnderWater = false;
            other.transform.GetChild(4).GetComponent<Animator>().SetBool(isThreading, false);
            //other.transform.GetChild(4).GetComponent<Animator>().SetBool(isWalking, false);
        }
    }
}
