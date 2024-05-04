using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerRaycast : NetworkBehaviour
{
    [SerializeField] private LineRenderer leftHandRaycast;
    [SerializeField] private NetworkedLineRenderer leftHandRaycastNetworked;
    [SerializeField] private LineRenderer rightHandRaycast;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Raycast();
    }

    private void Raycast()
    {
        LineRenderer leftHandRaycastReference = RaycastReferences.Instance.leftHandRaycast;
        LineRenderer rightHandRaycastReference = RaycastReferences.Instance.rightHandRaycast;

        Vector3[] leftHandRaycastPositions = new Vector3[leftHandRaycastReference.positionCount];
        leftHandRaycast.positionCount = leftHandRaycastReference.GetPositions(leftHandRaycastPositions);
        leftHandRaycastNetworked.SetPositionsServerRpc(leftHandRaycastPositions);

        Vector3[] rightHandRaycastPositions = new Vector3[rightHandRaycastReference.positionCount];
        rightHandRaycast.positionCount = rightHandRaycastReference.GetPositions(rightHandRaycastPositions);
        rightHandRaycast.SetPositions(rightHandRaycastPositions);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        leftHandRaycast.enabled = false;
        rightHandRaycast.enabled = false;
    }

    public void SetRaycastEnabled(bool enabled)
    {
        leftHandRaycast.enabled = enabled;
        rightHandRaycast.enabled = enabled;
    }
}
