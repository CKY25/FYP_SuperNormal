using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Nav : NetworkBehaviour
{
    private NavMeshAgent agent;
    private string isWalking = "isWalking";
    private float timePassed1 = 0f;
    private float timePassed2 = 0f;
    public float extraRotationSpeed = 3f;

    private void Awake()
    {
        transform.position = GetRandomSpawnPoint();
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        StartCoroutine(Roam());
    }

    private IEnumerator Roam()
    {
        while (true)
        {
            Vector3 point;
            if (RandomPoint(agent.transform.position, 100, out point)) // Random destination
            {
                if (agent.isActiveAndEnabled) 
                    agent.SetDestination(point);
                while (agent.isActiveAndEnabled && (agent.pathPending || agent.remainingDistance > agent.stoppingDistance))
                {
                    UpdateNPCAimatorServerRpc(GetComponent<NetworkObject>().NetworkObjectId, isWalking, true);
                    extraRotation();
                    yield return null; // Wait for the next frame
                }

                UpdateNPCAimatorServerRpc(GetComponent<NetworkObject>().NetworkObjectId, isWalking, false);

                // Stop for a random duration
                float stopDuration = Random.Range(1, 5);
                yield return new WaitForSeconds(stopDuration);
            }
        }
    }

    private bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }

    private Vector3 GetRandomSpawnPoint()
    {
        float randomX = Random.Range(25.45f, -32.2f);
        float randomY = Random.Range(2.89f, 2.89f);
        float randomZ = Random.Range(-40.9f, 22.44f);
        return new Vector3(randomX, randomY, randomZ);
    }

    private void extraRotation()
    {
        Vector3 lookrotation = agent.steeringTarget - transform.position;

        if (lookrotation.sqrMagnitude < Mathf.Epsilon)
        {
            return;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookrotation), extraRotationSpeed * Time.deltaTime);

    }

    [ServerRpc (RequireOwnership = false)]
    private void UpdateNPCAimatorServerRpc(ulong targetNetworkObjectId, string animationName, bool state)
    {
        UpdateNPCAimatorClientRpc(targetNetworkObjectId, animationName, state);
    }

    [ClientRpc]
    private void UpdateNPCAimatorClientRpc(ulong targetNetworkObjectId, string animationName, bool state)
    {
        NetworkObject targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetNetworkObjectId];
        GameObject target = targetNetworkObject.gameObject;
        target.GetComponent<Animator>().SetBool(animationName, state);
    }
}
