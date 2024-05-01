using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReplayControllerNetworkVariables : NetworkBehaviour
{
    public static ReplayControllerNetworkVariables Instance;
    private ReplayController replayController;

    public NetworkVariable<SaveFile> saveFile = new NetworkVariable<SaveFile>();
    public NetworkVariable<int> minPlayFrame = new NetworkVariable<int>();
    public NetworkVariable<int> maxPlayFrame = new NetworkVariable<int>();
    public NetworkVariable<int> activeFrame = new NetworkVariable<int>();
    public NetworkVariable<bool> isPlaying = new NetworkVariable<bool>();
    public NetworkVariable<Direction> direction = new NetworkVariable<Direction>();
    public NetworkVariable<bool> isLooping = new NetworkVariable<bool>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            replayController = ReplayController.Instance;

            replayController.OnReplayControllerLoaded += ReplayController_OnReplayLoad;
            replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;
            replayController.OnActiveFrameChanged += ReplayController_OnFrameChanged;
            replayController.OnPlay += ReplayController_OnPlay;
            replayController.OnPause += ReplayController_OnPause;
            replayController.OnDirectionChanged += ReplayController_OnDirectionChanged;
            replayController.OnRepeat += ReplayController_OnRepeat;
            replayController.OnReplayWindowSet += ReplayController_OnReplayWindowSet;
            replayController.OnReplayWindowReset += ReplayController_OnReplayWindowReset;
        }
    }

    private void ReplayController_OnReplayLoad(object sender, System.EventArgs e)
    {
        saveFile.Value = replayController.GetLoadedSaveFile();
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        saveFile.Value = SaveFile.None;
        minPlayFrame.Value = 0;
        maxPlayFrame.Value = 1;
        activeFrame.Value = 0;
        isPlaying.Value = false;
    }

    private void ReplayController_OnFrameChanged(object sender, ReplayController.OnActiveFrameChangedEventArgs e)
    {
        activeFrame.Value = e.newActiveFrame;
    }

    private void ReplayController_OnPlay(object sender, System.EventArgs e)
    {
        isPlaying.Value = true;
    }

    private void ReplayController_OnPause(object sender, System.EventArgs e)
    {
        isPlaying.Value = false;
    }

    private void ReplayController_OnDirectionChanged(object sender, ReplayController.OnDirectionChangedEventArgs e)
    {
        direction.Value = e.direction;
    }

    private void ReplayController_OnRepeat(object sender, System.EventArgs e)
    {
        isLooping.Value = replayController.IsLooping();
    }

    private void ReplayController_OnReplayWindowSet(object sender, ReplayController.OnReplayWindowSetEventArgs e)
    {
        minPlayFrame.Value = e.minReplayWindowFrame;
        maxPlayFrame.Value = e.maxReplayWindowFrame;
    }

    private void ReplayController_OnReplayWindowReset(object sender, System.EventArgs e)
    {
        minPlayFrame.Value = 0;
        maxPlayFrame.Value = replayController.GetMaxFrame();
    }
}
