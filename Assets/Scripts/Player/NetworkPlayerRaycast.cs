using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class NetworkPlayerRaycast : NetworkBehaviour
{
    [SerializeField] private bool syncLeftRaycast = true;
    [SerializeField] private LineRenderer leftHandRaycast;

    [SerializeField] private bool syncRightRaycast = true;
    [SerializeField] private LineRenderer rightHandRaycast;

    [SerializeField] private bool syncTeleportRaycast = false;
    [SerializeField] private LineRenderer teleportRaycast;

    public NetworkVariable<Vector3ArrayNetworkSerializable> leftRayPositions =
     new NetworkVariable<Vector3ArrayNetworkSerializable>(new Vector3ArrayNetworkSerializable());

    public NetworkVariable<Vector3ArrayNetworkSerializable> rightRayPositions =
    new NetworkVariable<Vector3ArrayNetworkSerializable>(new Vector3ArrayNetworkSerializable());

    public NetworkVariable<Vector3ArrayNetworkSerializable> teleportRayPositions =
    new NetworkVariable<Vector3ArrayNetworkSerializable>(new Vector3ArrayNetworkSerializable());

    public NetworkVariable<bool> leftRaycastEnabled = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> rightRaycastEnabled = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> teleportRaycastEnabled = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (syncLeftRaycast)
            {
                leftRayPositions.OnValueChanged += UpdateLeftRaycastPositions;
                leftRaycastEnabled.OnValueChanged += UpdateLeftRaycastEnabled;
                leftHandRaycast.positionCount = leftRayPositions.Value.GetPositions().Length;
                leftHandRaycast.SetPositions(leftRayPositions.Value.GetPositions());

                leftHandRaycast.enabled = leftRaycastEnabled.Value;
            }

            if (syncRightRaycast)
            {
                rightRayPositions.OnValueChanged += UpdateRightRaycastPositions;
                rightRaycastEnabled.OnValueChanged += UpdateRightRaycastEnabled;
                rightHandRaycast.positionCount = rightRayPositions.Value.GetPositions().Length;
                rightHandRaycast.SetPositions(rightRayPositions.Value.GetPositions());

                rightHandRaycast.enabled = rightRaycastEnabled.Value;
            }

            if (syncTeleportRaycast)
            {
                teleportRayPositions.OnValueChanged += UpdateTeleportRaycastPositions;
                teleportRaycastEnabled.OnValueChanged += UpdateTeleportRaycastEnabled;
                teleportRaycast.positionCount = teleportRayPositions.Value.GetPositions().Length;
                teleportRaycast.SetPositions(teleportRayPositions.Value.GetPositions());

                teleportRaycast.enabled = teleportRaycastEnabled.Value;
            }
        }
        else
        {
            if (syncLeftRaycast)
            {
                leftHandRaycast.enabled = RaycastReferences.Instance.leftHandRaycast.enabled;
            }
            else
            {
                leftHandRaycast.enabled = false;
            }

            if (syncRightRaycast)
            {
                rightHandRaycast.enabled = RaycastReferences.Instance.rightHandRaycast.enabled;
            }
            else
            {
                rightHandRaycast.enabled = false;
            }

            if (syncTeleportRaycast)
            {
                teleportRaycast.enabled = RaycastReferences.Instance.teleportRaycast.enabled;
            }
            else
            {
                teleportRaycast.enabled = false;
            }
        }
    }

    private void UpdateTeleportRaycastEnabled(bool previousValue, bool newValue)
    {
        if (teleportRaycast != null && !IsOwner)
        {
            teleportRaycast.enabled = newValue;
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

    private void UpdateTeleportRaycastPositions(Vector3ArrayNetworkSerializable previousPositions, Vector3ArrayNetworkSerializable newPositions)
    {
        if (teleportRaycast != null && !IsOwner)
        {
            teleportRaycast.positionCount = teleportRayPositions.Value.GetPositions().Length;
            teleportRaycast.SetPositions(teleportRayPositions.Value.GetPositions());
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
        if (syncLeftRaycast)
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
        }

        if (syncRightRaycast)
        {
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

        if (syncTeleportRaycast)
        {
            LineRenderer teleportRaycastReference = RaycastReferences.Instance.teleportRaycast;

            if (IsServer)
            {
                teleportRaycastEnabled.Value = teleportRaycastReference.enabled;
            }
            else
            {
                SetTeleportRaycastEnabledServerRpc(teleportRaycastReference.enabled);
            }

            if (teleportRaycastReference.enabled)
            {
                Vector3[] teleportRaycastPositions = new Vector3[teleportRaycastReference.positionCount];
                int positionCount = teleportRaycastReference.GetPositions(teleportRaycastPositions);

                Vector3ArrayNetworkSerializable teleportRaycastPositionsSerializable = new Vector3ArrayNetworkSerializable();
                teleportRaycastPositionsSerializable.Initialize(positionCount);
                teleportRaycastPositionsSerializable.SetPositions(teleportRaycastPositions);

                if (IsServer)
                {
                    teleportRayPositions.Value = teleportRaycastPositionsSerializable;
                }
                else
                {
                    SetTeleportRayPositionsServerRpc(teleportRaycastPositionsSerializable);
                }
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

    [ServerRpc(RequireOwnership = false)]
    private void SetTeleportRayPositionsServerRpc(Vector3ArrayNetworkSerializable teleportRaycastPositionsSerializable)
    {
        teleportRayPositions.Value = teleportRaycastPositionsSerializable;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetTeleportRaycastEnabledServerRpc(bool teleportRaycastEnabled)
    {
        this.teleportRaycastEnabled.Value = teleportRaycastEnabled;
    }

}
