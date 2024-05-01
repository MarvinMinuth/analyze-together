using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkArmMeshSync : NetworkBehaviour
{
    private NetworkVariable<bool> areMeshesShown = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            areMeshesShown.Value = GetComponent<DummyArmVisuals>().IsVisible();
            GetComponent<DummyArmVisuals>().OnVisibilityChanged += DummyArmVisuals_OnVisibilityChanged;
        }
        else
        {
            if (areMeshesShown.Value)
            {
                GetComponent<DummyArmVisuals>().Show();
            }
            else
            {
                GetComponent<DummyArmVisuals>().Hide();
            }

            areMeshesShown.OnValueChanged += OnMeshesShownChanged;
        }
    }

    private void DummyArmVisuals_OnVisibilityChanged(object sender, System.EventArgs e)
    {
        areMeshesShown.Value = GetComponent<DummyArmVisuals>().IsVisible();
    }

    private void OnMeshesShownChanged(bool previous, bool current)
    {

        Debug.Log("Change");

        if (current)
        {
            GetComponent<DummyArmVisuals>().Show();

        }
        else
        {
            GetComponent<DummyArmVisuals>().Hide();
        }
    }
}
