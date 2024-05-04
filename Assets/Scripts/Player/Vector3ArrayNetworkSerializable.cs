using Unity.Netcode;
using UnityEngine;

public class Vector3ArrayNetworkSerializable : INetworkSerializable
{
    private Vector3[] positions;

    // Parameterloser Konstruktor erforderlich für NetworkVariable
    public Vector3ArrayNetworkSerializable()
    {
        positions = new Vector3[0]; // Standardmäßig leer initialisieren
    }

    // Methode zum Initialisieren des Arrays mit einer bestimmten Größe
    public void Initialize(int size)
    {
        positions = new Vector3[size];
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = positions.Length;
        serializer.SerializeValue(ref length);
        if (serializer.IsReader)
        {
            positions = new Vector3[length];
        }

        for (int i = 0; i < length; i++)
        {
            serializer.SerializeValue(ref positions[i]);
        }
    }

    public Vector3[] GetPositions()
    {
        return positions;
    }

    public void SetPositions(Vector3[] newPositions)
    {
        positions = newPositions;
    }
}
