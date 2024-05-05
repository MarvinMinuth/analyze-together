using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerRaycast : NetworkBehaviour
{
    [SerializeField] private LineRenderer leftHandRaycast;
    [SerializeField] private LineRenderer rightHandRaycast;

    public NetworkVariable<Vector3ArrayNetworkSerializable> leftRayPositions =
     new NetworkVariable<Vector3ArrayNetworkSerializable>(new Vector3ArrayNetworkSerializable());

    public NetworkVariable<Vector3ArrayNetworkSerializable> rightRayPositions =
    new NetworkVariable<Vector3ArrayNetworkSerializable>(new Vector3ArrayNetworkSerializable());

    public NetworkVariable<bool> leftRaycastEnabled = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> rightRaycastEnabled = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            leftRayPositions.OnValueChanged += UpdateLeftRaycastPositions;
            rightRayPositions.OnValueChanged += UpdateRightRaycastPositions;

            leftRaycastEnabled.OnValueChanged += UpdateLeftRaycastEnabled;
            rightRaycastEnabled.OnValueChanged += UpdateRightRaycastEnabled;

            leftHandRaycast.positionCount = leftRayPositions.Value.GetPositions().Length;
            leftHandRaycast.SetPositions(leftRayPositions.Value.GetPositions());

            rightHandRaycast.positionCount = rightRayPositions.Value.GetPositions().Length;
            rightHandRaycast.SetPositions(rightRayPositions.Value.GetPositions());

            leftHandRaycast.enabled = leftRaycastEnabled.Value;
            rightHandRaycast.enabled = rightRaycastEnabled.Value;
        }
        else
        {
            leftHandRaycast.enabled = false;
            rightHandRaycast.enabled = false;
        }
    }

    private void UpdateLeftRaycastEnabled(bool previousValue, bool newValue)
    {
        if (leftHandRaycast != null && !IsOwner)
        {
            leftHandRaycast.enabled = newValue;
        }
    }

    private void UpdateRightRaycastEnabled(bool previousValue, bool newValue)
    {
        if (rightHandRaycast != null && !IsOwner)
        {
            rightHandRaycast.enabled = newValue;
        }
    }

    private void UpdateLeftRaycastPositions(Vector3ArrayNetworkSerializable previousPositions, Vector3ArrayNetworkSerializable newPositions)
    {
        if (leftHandRaycast != null && !IsOwner)
        {
            leftHandRaycast.positionCount = leftRayPositions.Value.GetPositions().Length;
            leftHandRaycast.SetPositions(leftRayPositions.Value.GetPositions());
        }
    }

    private void UpdateRightRaycastPositions(Vector3ArrayNetworkSerializable previousPositions, Vector3ArrayNetworkSerializable newPositions)
    {
        if (rightHandRaycast != null && !IsOwner)
        {
            rightHandRaycast.positionCount = rightRayPositions.Value.GetPositions().Length;
            rightHandRaycast.SetPositions(rightRayPositions.Value.GetPositions());
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        else
        {
            Raycast();
        }
    }

    private void Raycast()
    {
        LineRenderer leftHandRaycastReference = RaycastReferences.Instance.leftHandRaycast;

        if (IsServer)
        {
            leftRaycastEnabled.Value = leftHandRaycastReference.enabled;
        }
        else
        {
            SetLeftRaycastEnabledServerRpc(leftHandRaycastReference.enabled);
        }

        if (leftHandRaycastReference.enabled)
        {
            Vector3[] leftHandRaycastPositions = new Vector3[leftHandRaycastReference.positionCount];
            int positionCount = leftHandRaycastReference.GetPositions(leftHandRaycastPositions);

            Vector3ArrayNetworkSerializable leftHandRaycastPositionsSerializable = new Vector3ArrayNetworkSerializable();
            leftHandRaycastPositionsSerializable.Initialize(positionCount);
            leftHandRaycastPositionsSerializable.SetPositions(leftHandRaycastPositions);

            if (IsServer)
            {
                leftRayPositions.Value = leftHandRaycastPositionsSerializable;
            }
            else
            {
                SetLeftRayPositionsServerRpc(leftHandRaycastPositionsSerializable);
            }
        }


        LineRenderer rightHandRaycastReference = RaycastReferences.Instance.rightHandRaycast;

        if (IsServer)
        {
            rightRaycastEnabled.Value = rightHandRaycastReference.enabled;
        }
        else
        {
            SetRightRaycastEnabledServerRpc(rightHandRaycastReference.enabled);
        }

        if (rightHandRaycastReference.enabled)
        {
            Vector3[] rightHandRaycastPositions = new Vector3[rightHandRaycastReference.positionCount];
            int positionCount = rightHandRaycastReference.GetPositions(rightHandRaycastPositions);

            Vector3ArrayNetworkSerializable rightHandRaycastPositionsSerializable = new Vector3ArrayNetworkSerializable();
            rightHandRaycastPositionsSerializable.Initialize(positionCount);
            rightHandRaycastPositionsSerializable.SetPositions(rightHandRaycastPositions);

            if (IsServer)
            {
                rightRayPositions.Value = rightHandRaycastPositionsSerializable;
            }
            else
            {
                SetRightRayPositionsServerRpc(rightHandRaycastPositionsSerializable);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetLeftRayPositionsServerRpc(Vector3ArrayNetworkSerializable leftHandRaycastPositionsSerializable)
    {
        leftRayPositions.Value = leftHandRaycastPositionsSerializable;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRightRayPositionsServerRpc(Vector3ArrayNetworkSerializable rightHandRaycastPositionsSerializable)
    {
        rightRayPositions.Value = rightHandRaycastPositionsSerializable;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetLeftRaycastEnabledServerRpc(bool leftRaycastEnabled)
    {
        this.leftRaycastEnabled.Value = leftRaycastEnabled;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRightRaycastEnabledServerRpc(bool rightRaycastEnabled)
    {
        this.rightRaycastEnabled.Value = rightRaycastEnabled;
    }
}
