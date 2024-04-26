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

    protected override void OnRecordingLoadedChanged(bool previous, bool current)
    {
        base.OnRecordingLoadedChanged(previous, current);
        if (current)
        {
            slider.maxValue = variableSync.replayLength.Value;
            slider.minValue = variableSync.minFrame.Value;
            slider.value = variableSync.activeFrame.Value;
        }
        else
        {
            slider.maxValue = 1;
            slider.minValue = 0;
            slider.value = 0;
        }
    }
    protected override void SetMinAccessibleValue(float value)
    {
        //if (!timelineSet) return;
        SetMinValue(value);
        base.SetMinAccessibleValue(value);
        slider.minValue = value;
    }
    protected override void SetMaxAccessibleValue(float value)
    {
        //if (!timelineSet) return;
        SetMaxValue(value);
        base.SetMaxAccessibleValue(value);
        slider.maxValue = value;
    }
    private void OnActiveFrameChanged(int previous, int current)
    {
        if (!inUse)
        {
            slider.value = current;
        }
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (!inUse && current)
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
            moreThanOneInteractor = true;
            return;
        }

        inUse = true;
        interactionCoordinator.Lock();
        variableSync.RequestAccessServerRpc();

        //if (!variableSync.IsInteractor(NetworkManager.LocalClientId)) { return; }


        wasRunning = variableSync.isPlaying.Value;
        controlRpcs.PauseServerRpc();
        //replayController.SetReceivingInput(true);
    }

    public void EndDrag()
    {
        //if (!variableSync.IsInteractor(NetworkManager.LocalClientId)) { return; }
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
