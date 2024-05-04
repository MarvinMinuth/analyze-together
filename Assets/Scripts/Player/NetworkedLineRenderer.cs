using Unity.Netcode;
using UnityEngine;

public class NetworkedLineRenderer : NetworkBehaviour
{
    private LineRenderer lineRenderer;

    public NetworkVariable<Vector3ArrayNetworkSerializable> LinePositions =
     new NetworkVariable<Vector3ArrayNetworkSerializable>(new Vector3ArrayNetworkSerializable());


    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            //Vector3ArrayNetworkSerializable initialPositions = new Vector3ArrayNetworkSerializable();
            //initialPositions.Initialize(2); // Oder eine andere Logik zur Bestimmung der Größe
            //LinePositions.Value = initialPositions;

            Vector3[] initialPositions = new Vector3[2];
            initialPositions[0] = Vector3.zero;
            initialPositions[1] = Vector3.one;
            SetPositionsServerRpc(initialPositions);
        }

        LinePositions.OnValueChanged += UpdateLineRendererPositions;
    }

    private void UpdateLineRendererPositions(Vector3ArrayNetworkSerializable previousPositions, Vector3ArrayNetworkSerializable newPositions)
    {
        if (lineRenderer != null)
        {
            SetPositionsServerRpc(newPositions.GetPositions());
        }
    }

    [ServerRpc]
    public void SetPositionsServerRpc(Vector3[] positions)
    {
        Vector3ArrayNetworkSerializable newPos = new Vector3ArrayNetworkSerializable();
        newPos.Initialize(positions.Length);
        newPos.SetPositions(positions);
        LinePositions.Value = newPos;
    }
}
