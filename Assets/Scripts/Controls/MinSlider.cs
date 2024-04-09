using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MinSlider : NetworkBehaviour
{
    [SerializeField] private ReplayWindowController replayWindowController;
    [SerializeField] private Slider minSlider;

    private NetworkVariableSync variableSync;
    private ReplayControlRpcs controlRpcs;
    private float length;
    private InteractionCoordinator interactionCoordinator;
    private bool moreThanOneInteractor;
    private bool wasRunning;
    private bool inUse;

    private void Start()
    {
        interactionCoordinator = InteractionCoordinator.Instance;

        inUse = false;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        variableSync = NetworkVariableSync.Instance;
        controlRpcs = ReplayControlRpcs.Instance;

        variableSync.replayLength.OnValueChanged += OnReplayLengthChanged;
        variableSync.minFrame.OnValueChanged += OnMinFrameChanged;

        variableSync.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;

    }

    private void OnReplayLengthChanged(int previous, int current)
    {
        length = current;
        minSlider.maxValue = current;
        minSlider.value = 0;
    }

    private void OnMinFrameChanged(int previous, int current)
    {
        minSlider.value = current;
    }

    public void ChangeReplayWindow()
    {
        replayWindowController.ChangeReplayWindow();
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (interactionCoordinator.IsLocked() && current && !inUse)
        {
            minSlider.interactable = false;
        }
        else
        {
            minSlider.interactable = true;
        }
    }

    public void StartDrag()
    {
        if (interactionCoordinator.IsLocked())
        {
            Debug.Log("Locked");
            moreThanOneInteractor = true;
            return;
        }

        inUse = true;
        interactionCoordinator.Lock();
        variableSync.RequestAccessServerRpc();

        //if (!variableSync.IsInteractor(NetworkManager.LocalClientId)) { return; }

        Debug.Log("Start Drag");


        wasRunning = variableSync.isPlaying.Value;
        Debug.Log("Was Running: " + wasRunning);
        controlRpcs.PauseServerRpc();
        //replayController.SetReceivingInput(true);
    }

    public void EndDrag()
    {
        //if (!variableSync.IsInteractor(NetworkManager.LocalClientId)) { return; }
        Debug.Log("End Drag");
        if (moreThanOneInteractor)
        {
            moreThanOneInteractor = false;
            return;
        }

        if (wasRunning)
        {
            controlRpcs.PlayServerRpc();
        }
        //replayController.SetReceivingInput(false);
        inUse = false;
        variableSync.FreeAccessServerRpc();
    }

    public void OnValueChanged()
    {
        if (!inUse || moreThanOneInteractor) { return; }

        float min = minSlider.value;
        float max = variableSync.maxFrame.Value;

        if (max - min < replayWindowController.GetMinDistance())
        {
            minSlider.value = max - replayWindowController.GetMinDistance();
        }

        ChangeReplayWindow();
    }
}
