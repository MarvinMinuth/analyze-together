using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * This Timeline has a Slider which min and max values sync with the min and max value of the replay
 */
public class OverviewSliderTimeline : TimelineNew
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

    protected override void SetMinValue(float value)
    {
        base.SetMinValue(value);
        minAccessibleValue = value;
        slider.minValue = value;
    }

    protected override void SetMaxValue(float value)
    {
        value = value - 1;
        base.SetMaxValue(value);
        maxAccessibleValue = value;
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
        if (!inUse && current && interactionCoordinator.IsLocked())
        {
            Debug.Log(variableSync.interactorId.Value + " " + NetworkManager.LocalClientId);
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

        if (slider.value < minAccessibleValue) { slider.value = minAccessibleValue; }
        if (slider.value > maxAccessibleValue) { slider.value = maxAccessibleValue; }

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

        /*
        int closestHighlight = minFrame;
        if(shownHighlights.Count != 0) { closestHighlight = shownHighlights.OrderBy(x => Mathf.Abs((long)x - value)).First(); }


        // �berpr�fe, ob ein Highlight in der N�he des neuen Werts existiert
        if (Mathf.Abs(closestHighlight - value) <= highlightThreshold)
        {
            // Setze den Replay-Zeitpunkt auf den n�chsten Highlight-Wert
            activeFrame = closestHighlight;
            replayController.SetFrame(activeFrame);
        }
        else
        {
            // Bestimme die Differenz zwischen dem aktuellen Wert und dem neuen Wert
            float valueDifference = Mathf.Abs(activeFrame - value);

            // �berpr�fe, ob die Differenz gr��er als eine bestimmte Schwelle ist
            if (valueDifference > threshold)
            {
                // Setze den Replay-Zeitpunkt auf den Wert des Timelines
                activeFrame = (int)value;
                replayController.SetFrame(activeFrame);
            }
        }
        */
    }

}
