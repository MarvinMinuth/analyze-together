using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MaxSlider : NetworkBehaviour
{
    [SerializeField] private ReplayWindowController replayWindowController;
    [SerializeField] private Slider maxSlider;

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
        variableSync.maxFrame.OnValueChanged += OnMaxFrameChanged;

        variableSync.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;

    }

    private void OnReplayLengthChanged(int previous, int current)
    {
        length = current;
        maxSlider.maxValue = current;
        maxSlider.value = current;
    }

    private void OnMaxFrameChanged(int previous, int current)
    {
        maxSlider.value = current;
    }

    public void ChangeReplayWindow()
    {
        replayWindowController.ChangeReplayWindow();
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (interactionCoordinator.IsLocked() && current && !inUse)
        {
            Debug.Log(variableSync.interactorId.Value + " " + NetworkManager.LocalClientId);
            maxSlider.interactable = false;
        }
        else
        {
            maxSlider.interactable = true;
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

        float min = variableSync.minFrame.Value;
        float max = maxSlider.value;

        if (max - min < replayWindowController.GetMinDistance())
        {
            maxSlider.value = min + replayWindowController.GetMinDistance();
        }

        ChangeReplayWindow();
    }
}
