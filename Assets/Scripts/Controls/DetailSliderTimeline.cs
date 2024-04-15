using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetailSliderTimeline : TimelineNew
{
    [SerializeField] private Slider slider;
    [SerializeField] private float threshold = 30;

    private bool wasRunning;
    private bool moreThanOneInteractor;

    private ReplayControlRpcs controlRpcs;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        controlRpcs = ReplayControlRpcs.Instance;

        variableSync.activeFrame.OnValueChanged += OnActiveFrameChanged;
        variableSync.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
    }
    protected override void SetMinAccessibleValue(float value)
    {
        SetMinValue(value);
        base.SetMinAccessibleValue(value);
        slider.minValue = value;
    }
    protected override void SetMaxAccessibleValue(float value)
    {
        SetMaxValue(value);
        base.SetMaxAccessibleValue(value);
        slider.maxValue = value;
    }
    private void OnActiveFrameChanged(int previous, int current)
    {
        if (!interactionCoordinator.IsLocked())
        {
            slider.value = current;
        }
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (!inUse && current && interactionCoordinator.IsLocked())
        {
            slider.interactable = false;
        }
        else
        {
            slider.interactable = true;
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

    public void OnTimelineValueChanged()
    {
        if (!inUse || moreThanOneInteractor) { return; }

        float value = slider.value;

        // estimate difference between current frame and new value
        float valueDifference = Mathf.Abs(activeFrame - value);

        // check if difference is greater than threshold
        if (valueDifference > threshold)
        {
            // set replay time to value of timeline
            activeFrame = (int)value;
            controlRpcs.SetFrameServerRpc(activeFrame);
        }
    }
}
