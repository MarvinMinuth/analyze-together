using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkVariableSync : NetworkBehaviour
{
    public static NetworkVariableSync Instance;

    private ReplayController replayController;

    public NetworkVariable<Savefile> savefile = new NetworkVariable<Savefile>();
    public NetworkVariable<int> minFrame = new NetworkVariable<int>();
    public NetworkVariable<int> maxFrame = new NetworkVariable<int>();
    public NetworkVariable<int> activeFrame = new NetworkVariable<int>();
    public NetworkVariable<int> replayLength = new NetworkVariable<int>();
    public NetworkVariable<bool> isPlaying = new NetworkVariable<bool>();
    public NetworkVariable<Direction> direction = new NetworkVariable<Direction>();
    public NetworkVariable<bool> isLooping = new NetworkVariable<bool>();

    public NetworkVariable<bool> isInteractionInProgress = new NetworkVariable<bool>();
    public NetworkVariable<ulong> interactorId = new NetworkVariable<ulong>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {

    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            replayController = ReplayController.Instance;

            replayController.OnReplayControllerLoaded += ReplayController_OnReplayControllerLoaded;
            replayController.OnFrameChanged += ReplayController_OnFrameChanged;
            replayController.OnPlay += ReplayController_OnPlay;
            replayController.OnPause += ReplayController_OnPause;
            replayController.OnDirectionChanged += ReplayController_OnDirectionChanged;
            replayController.OnRepeat += ReplayController_OnRepeat;
            replayController.OnReplayWindowSet += ReplayController_OnReplayWindowSet;
            replayController.OnReplayWindowReset += ReplayController_OnReplayWindowReset;
        }
    }

    private void ReplayController_OnReplayWindowReset(object sender, System.EventArgs e)
    {
        minFrame.Value = 0;
        maxFrame.Value = replayController.GetReplayLength();
    }
    private void ReplayController_OnReplayWindowSet(object sender, ReplayController.OnReplayWindowSetEventArgs e)
    {
        minFrame.Value = e.minReplayWindowFrame;
        maxFrame.Value = e.maxReplayWindowFrame;
    }

    private void ReplayController_OnReplayControllerLoaded(object sender, System.EventArgs e)
    {
        savefile.Value = RecordingManager.Instance.GetActiveReplaySO().savefile;
        replayLength.Value = replayController.GetReplayLength();
    }

    private void ReplayController_OnFrameChanged(object sender, ReplayController.OnFrameChangedEventArgs e)
    {
        activeFrame.Value = replayController.GetFrame();
    }

    private void ReplayController_OnRepeat(object sender, System.EventArgs e)
    {
        isLooping.Value = replayController.IsLooping();
    }

    private void ReplayController_OnDirectionChanged(object sender, ReplayController.OnDirectionChangedEventArgs e)
    {
        direction.Value = e.direction;
    }

    private void ReplayController_OnPause(object sender, System.EventArgs e)
    {
        isPlaying.Value = false;
    }

    private void ReplayController_OnPlay(object sender, System.EventArgs e)
    {
        isPlaying.Value = true;
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

    [ServerRpc(RequireOwnership = false)]
    public void SetFrameServerRpc(float frame, ServerRpcParams serverRpcParams = default)
    {
        if (isInteractionInProgress.Value && interactorId.Value != serverRpcParams.Receive.SenderClientId) { return; }
        replayController.SetFrame((int)frame);
    }

    public bool RequestTimelineLock(ulong userId)
    {
        RequestAccessServerRpc();
        if (IsInteractor(userId)) { return true; }
        return false;
    }
}
