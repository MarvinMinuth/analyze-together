using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Timeline : MonoBehaviour
{
    public static event EventHandler<OnTimelineUsedEventArgs> OnTimelineUsed;
    public class OnTimelineUsedEventArgs
    {
        public Timeline usedTimeline;
    }
    public static event EventHandler OnTimelineFreed;

    protected virtual void Start()
    {
        Timeline.OnTimelineUsed += Timeline_OnTimelineUsed;
        Timeline.OnTimelineFreed += Timeline_OnTimelineFreed;
    }

    protected abstract void Timeline_OnTimelineFreed(object sender, EventArgs e);

    protected abstract void Timeline_OnTimelineUsed(object sender, OnTimelineUsedEventArgs e);

    protected abstract void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e);

    public abstract void StartDrag();

    public abstract void EndDrag();

    protected void TriggerOnTimelineUsed(Timeline timeline)
    {
        OnTimelineUsed?.Invoke(timeline, new OnTimelineUsedEventArgs
        {
            usedTimeline = timeline
        });
    }

    protected void TriggerOnTimelineFreed()
    {
        OnTimelineFreed?.Invoke(this, EventArgs.Empty);
    }
}
