using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class NetworkReplayController : NetworkBehaviour
{
    private ReplayController replayController;

    public event EventHandler<OnReplayLoadedEventArgs> OnReplayLoaded;
    public class OnReplayLoadedEventArgs
    {
        public RecordingSO activeReplaySO;
    }

    private void Awake()
    {
    }

    public override void OnNetworkSpawn()
    {
        replayController = ReplayController.Instance;

        replayController.OnReplayControllerLoaded += ReplayController_OnReplayControllerLoaded;

    }

    private void ReplayController_OnReplayControllerLoaded(object sender, System.EventArgs e)
    {
        RecordingSO replaySO = RecordingManager.Instance.GetActiveReplaySO();

        OnReplayLoaded?.Invoke(this, new OnReplayLoadedEventArgs
        {
            activeReplaySO = replaySO
        });
    }
}
