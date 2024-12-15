using UnityEngine;
using Unity.Netcode;

public class GameTimer : NetworkBehaviour
{
    public float timeLimit = 600f; // 10 minutes in seconds
    private float currentTime;

    public NetworkVariable<float> networkCurrentTime = new NetworkVariable<float>(600f);

    private void Awake()
    {
        currentTime = timeLimit;
    }

    void Start()
    {
        if (IsServer)
        {
            currentTime = timeLimit;
            networkCurrentTime.Value = currentTime;
        }
    }

    void Update()
    {
        if (IsServer)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                networkCurrentTime.Value = currentTime;
            }
        }
    }
}
