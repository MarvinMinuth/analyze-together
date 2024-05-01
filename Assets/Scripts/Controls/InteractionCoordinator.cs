using System;
using Unity.Netcode;

public class InteractionCoordinator : NetworkBehaviour
{
    public static InteractionCoordinator Instance;
    public NetworkVariable<bool> isInteractionInProgress;
    public NetworkVariable<ulong> interactorId;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    public void InitStartInteraction()
    {
        if (IsServer)
        {
            StartInteraction();
        }
        else
        {
            RequestAccessServerRpc();
        }
    }

    private void StartInteraction()
    {
        if (isInteractionInProgress.Value) { return; }
        interactorId.Value = NetworkManager.LocalClientId;
        isInteractionInProgress.Value = true;
    }

    public void EndInteraction()
    {
        if (IsServer)
        {
            isInteractionInProgress.Value = false;
        }
        else
        {
            FreeAccessServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestAccessServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!isInteractionInProgress.Value)
        {
            interactorId.Value = serverRpcParams.Receive.SenderClientId;
            isInteractionInProgress.Value = true;
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void FreeAccessServerRpc()
    {
        isInteractionInProgress.Value = false;
    }

    public bool IsInteractor(ulong senderId)
    {
        return (isInteractionInProgress.Value && senderId == interactorId.Value);
    }
}
