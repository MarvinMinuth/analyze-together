using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MoveableObjectSync : NetworkBehaviour
{
    [SerializeField] private MoveableOnTrajectories moveableObject;

    public NetworkVariable<float> x = new NetworkVariable<float>();
    public NetworkVariable<float> y = new NetworkVariable<float>();
    public NetworkVariable<float> z = new NetworkVariable<float>();

    public NetworkVariable<float> xRot = new NetworkVariable<float>();
    public NetworkVariable<float> yRot = new NetworkVariable<float>();
    public NetworkVariable<float> zRot = new NetworkVariable<float>();

    public NetworkVariable<bool> isHidden = new NetworkVariable<bool>();

    private void Update()
    {
        if (isHidden.Value) { return; }

        if (IsServer)
        {
            x.Value = moveableObject.transform.position.x;
            y.Value = moveableObject.transform.position.y;
            z.Value = moveableObject.transform.position.z;

            xRot.Value = moveableObject.transform.rotation.eulerAngles.x;
            yRot.Value = moveableObject.transform.rotation.eulerAngles.y;
            zRot.Value = moveableObject.transform.rotation.eulerAngles.z;

            isHidden.Value = moveableObject.IsHidden;
        }
        else
        {
            if (moveableObject.IsDragged)
            {
                return;
            }
            moveableObject.SetPosition(new Vector3(x.Value, y.Value, z.Value), Quaternion.Euler(xRot.Value, yRot.Value, zRot.Value));
            moveableObject.ChangeHidden(isHidden.Value);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            moveableObject.SetPosition(new Vector3(x.Value, y.Value, z.Value), Quaternion.Euler(xRot.Value, yRot.Value, zRot.Value));
            moveableObject.ChangeHidden(isHidden.Value);
        }
        else
        {
            x.Value = moveableObject.transform.position.x;
            y.Value = moveableObject.transform.position.y;
            z.Value = moveableObject.transform.position.z;

            xRot.Value = moveableObject.transform.rotation.eulerAngles.x;
            yRot.Value = moveableObject.transform.rotation.eulerAngles.y;
            zRot.Value = moveableObject.transform.rotation.eulerAngles.z;

            isHidden.Value = moveableObject.IsHidden;
        }
    }

    public void SetPosition(Vector3 position, Quaternion rotation)
    {
        if (IsServer)
        {
            return;
        }
        SetPositionServerRpc(position, rotation);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPositionServerRpc(Vector3 position, Quaternion rotation)
    {
        moveableObject.transform.position = position;
        moveableObject.transform.rotation = rotation;
    }

    public void ChangeHidden(bool hidden)
    {
        if (IsServer)
        {
            moveableObject.ChangeHidden(hidden);
        }
        ChangeHiddenServerRpc(hidden);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHiddenServerRpc(bool hidden)
    {
        moveableObject.ChangeHidden(hidden);
    }

}
