using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkVisualsSync : NetworkBehaviour
{
    private NetworkVariable<bool> isVisible = new NetworkVariable<bool>();

    private IVisuals visuals;

    public override void OnNetworkSpawn()
    {
        visuals = GetComponent<IVisuals>();

        if (IsServer)
        {
            isVisible.Value = visuals.IsVisible();
            visuals.OnVisibilityChanged += DummyArmVisuals_OnVisibilityChanged;
        }
        else
        {
            if (isVisible.Value)
            {
                visuals.Show();
            }
            else
            {
                visuals.Hide();
            }

            isVisible.OnValueChanged += OnMeshesShownChanged;
        }
    }

    private void DummyArmVisuals_OnVisibilityChanged(object sender, System.EventArgs e)
    {
        isVisible.Value = visuals.IsVisible();
    }

    private void OnMeshesShownChanged(bool previous, bool isVisible)
    {
        if (isVisible)
        {
            visuals.Show();

        }
        else
        {
            visuals.Hide();
        }
    }
}
