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
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = activeFrame;
    }

    protected override void SetMinValue(float value)
    {
        base.SetMinValue(value);
        minAccessibleValue = value;
        slider.minValue = value;
    }

    protected override void SetMaxValue(float value)
    {
        base.SetMaxValue(value);
        maxAccessibleValue = value;
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
            replayController.InitSetFrame(activeFrame);
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
