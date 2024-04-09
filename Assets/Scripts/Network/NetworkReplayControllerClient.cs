using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkReplayControllerClient : NetworkBehaviour
{

    private NetworkReplayControllerServer replayControllerServer;

    public override void OnNetworkSpawn()
    {
        replayControllerServer = NetworkReplayControllerServer.Instance;

        RequestStateServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestStateServerRpc()
    {
        replayControllerServer.SendState(OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void LoadTutorialServerRpc()
    {
        replayControllerServer.LoadTutorialRequest(OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayServerRpc()
    {
        replayControllerServer.PlayRequest();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PauseServerRpc()
    {
        replayControllerServer.PauseRequest();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetFrameServerRpc(int frame)
    {
        replayControllerServer.SetFrameRequest(frame);
    }
}
