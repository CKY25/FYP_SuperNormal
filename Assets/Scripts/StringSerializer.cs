using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StringSerializer : INetworkSerializable
{
    public string chosenAbilityEnum;

    public StringSerializer()
    {
        chosenAbilityEnum = "";
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref chosenAbilityEnum);
    }
}
