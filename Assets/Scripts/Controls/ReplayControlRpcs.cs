using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReplayControlRpcs : NetworkBehaviour
{
    public static ReplayControlRpcs Instance;

    private ReplayController replayController;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        replayController = ReplayController.Instance;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayServerRpc()
    {
        replayController.InitPlay();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PauseServerRpc()
    {
        replayController.InitPause();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopServerRpc()
    {
        replayController.InitStop();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeDirectionServerRpc()
    {
        replayController.InitChangeDirection();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeDirectionServerRpc(Direction direction)
    {
        replayController.InitChangeDirection(direction);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepeatServerRpc()
    {
        replayController.InitChangeLooping();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepeatServerRpc(bool shouldLoop)
    {
        replayController.InitChangeLooping(shouldLoop);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetFrameServerRpc(int frame)
    {
        replayController.InitSetFrame(frame);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeReplayWindowServerRpc(int minFrame, int maxFrame)
    {
        replayController.InitChangeReplayWindow(minFrame, maxFrame);
    }
}
