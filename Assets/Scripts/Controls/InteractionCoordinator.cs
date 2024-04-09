using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InteractionCoordinator : NetworkBehaviour
{
    public static InteractionCoordinator Instance;
    private bool isLocked;

    private NetworkVariableSync variableSync;

    private void Awake()
    {
        Instance = this;
        isLocked = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        variableSync = NetworkVariableSync.Instance;

        variableSync.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
        isLocked = variableSync.isInteractionInProgress.Value;
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        isLocked = current;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public void Lock()
    {
        isLocked = true;
    }
}
