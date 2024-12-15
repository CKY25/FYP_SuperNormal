using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct AbilityData : INetworkSerializable
{
    public string name;
    public float cdTime;
    public float activeTime;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref cdTime);
        serializer.SerializeValue(ref activeTime);
    }
}
