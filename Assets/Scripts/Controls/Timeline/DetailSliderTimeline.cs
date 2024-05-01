using System;
using UnityEngine;
using UnityEngine.UI;

public class DetailSliderTimeline : TimelineNew
{
    [SerializeField] private Slider slider;
    [SerializeField] private float threshold = 5;

    private bool wasRunning;
    private bool moreThanOneInteractor;

    protected override void InteractionCoordinator_OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (!inUse)
        {
            slider.interactable = !current;
        }
    }
    protected override void ReplayController_OnActiveFrameChanged(object sender, ReplayController.OnActiveFrameChangedEventArgs e)
    {
        base.ReplayController_OnActiveFrameChanged(sender, e);
        if (!inUse)
        {
            slider.value = e.newActiveFrame;
        }
    }

    protected override void ResetTimelineValues()
    {
        base.ResetTimelineValues();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;
    }

    protected override void InitializeTimelineValues(int minValue, int maxValue, int minAccessibleValue, int maxAccessibleValue, int activeFrame)
    {
        base.InitializeTimelineValues(minValue, maxValue, minAccessibleValue, maxAccessibleValue, activeFrame);
        slider.minValue = minAccessibleValue;
        slider.maxValue = maxAccessibleValue;
        slider.value = activeFrame;
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
        if (slider.interactable == false)
        {
            return;
        }
        if (inUse)
        {
            moreThanOneInteractor = true;
            return;
        }

        inUse = true;

        interactionCoordinator.InitStartInteraction();

        wasRunning = replayController.IsPlaying();
        replayController.InitPause();
    }

    public void EndDrag()
    {
        if (!inUse || slider.interactable == false)
        {
            return;
        }
        if (moreThanOneInteractor)
        {
            moreThanOneInteractor = false;
            return;
        }

        if (wasRunning)
        {
            replayController.InitPlay();
        }

        inUse = false;
        interactionCoordinator.EndInteraction();
    }

    public void OnTimelineValueChanged()
    {
        if (!inUse)
        {
            slider.value = activeFrame;
            return;
        }

        float value = slider.value;

        // estimate difference between current frame and new value
        float valueDifference = Mathf.Abs(activeFrame - value);

        // check if difference is greater than threshold
        if (valueDifference > threshold)
        {
            // set replay time to value of timeline
            activeFrame = (int)value;
            replayController.InitSetFrame(activeFrame);
        }
    }
}
