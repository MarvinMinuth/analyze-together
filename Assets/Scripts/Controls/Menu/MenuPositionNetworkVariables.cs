using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MenuPositionNetworkVariables : NetworkBehaviour
{
    public NetworkVariable<float> x = new NetworkVariable<float>();
    public NetworkVariable<float> y = new NetworkVariable<float>();
    public NetworkVariable<float> z = new NetworkVariable<float>();

    public NetworkVariable<float> xRot = new NetworkVariable<float>();
    public NetworkVariable<float> yRot = new NetworkVariable<float>();
    public NetworkVariable<float> zRot = new NetworkVariable<float>();

    [SerializeField] private MenuCube menuCube;

    private void Update()
    {
        if (IsServer)
        {
            x.Value = menuCube.transform.position.x;
            y.Value = menuCube.transform.position.y;
            z.Value = menuCube.transform.position.z;

            xRot.Value = menuCube.transform.rotation.eulerAngles.x;
            yRot.Value = menuCube.transform.rotation.eulerAngles.y;
            zRot.Value = menuCube.transform.rotation.eulerAngles.z;
        }
        else
        {
            if (menuCube.IsDragged)
            {
                return;
            }
            menuCube.SetPosition(new Vector3(x.Value, y.Value, z.Value), Quaternion.Euler(xRot.Value, yRot.Value, zRot.Value));
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            menuCube.SetPosition(new Vector3(x.Value, y.Value, z.Value), Quaternion.Euler(xRot.Value, yRot.Value, zRot.Value));
        }
        else
        {
            x.Value = menuCube.transform.position.x;
            y.Value = menuCube.transform.position.y;
            z.Value = menuCube.transform.position.z;

            xRot.Value = menuCube.transform.rotation.eulerAngles.x;
            yRot.Value = menuCube.transform.rotation.eulerAngles.y;
            zRot.Value = menuCube.transform.rotation.eulerAngles.z;
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
        menuCube.transform.position = position;
        menuCube.transform.rotation = rotation;
    }
}
