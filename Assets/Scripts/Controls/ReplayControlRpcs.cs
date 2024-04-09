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
        replayController.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PauseServerRpc()
    {
        replayController.Pause();
    }

    [ServerRpc(RequireOwnership = false)]
    public void StopServerRpc()
    {
        replayController.Stop();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeDirectionServerRpc()
    {
        replayController.ChangeDirection();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RepeatServerRpc()
    {
        replayController.ChangeLooping();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetFrameServerRpc(int frame)
    {
        replayController.SetFrame(frame);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeReplayWindowServerRpc(int minFrame, int maxFrame)
    {
        replayController.ChangeReplayWindow(minFrame, maxFrame);
    }
}
