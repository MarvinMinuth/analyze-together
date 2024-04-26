using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/*
 * Every Timeline is a visualization of the ReplayController
 * When the network spawns the Timeline syncs with the relevant values
 */
public class TimelineNew : NetworkBehaviour
{
    public event EventHandler OnTimelineLoaded;
    public event EventHandler OnTimelineChanged;
    public event EventHandler OnTimelineReset;
    protected int activeFrame;
    protected float minValue, maxValue;
    protected float minAccessibleValue, maxAccessibleValue;
    protected InteractionCoordinator interactionCoordinator;

    private ReplayController replayController;
    protected NetworkVariableSync variableSync;

    protected bool inUse;

    protected bool timelineSet;


    private void Start()
    {
        interactionCoordinator = InteractionCoordinator.Instance;

        inUse = false;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        variableSync = NetworkVariableSync.Instance;

        variableSync.isRecordingLoaded.OnValueChanged += OnRecordingLoadedChanged;
        variableSync.replayLength.OnValueChanged += OnReplayLengthChanged;
        variableSync.minFrame.OnValueChanged += OnMinFrameChanged;
        variableSync.maxFrame.OnValueChanged += OnMaxFrameChanged;

        SetMaxValue(variableSync.replayLength.Value);
        SetMinValue(0);
        SetMaxAccessibleValue(variableSync.maxFrame.Value);
        SetMinAccessibleValue(variableSync.minFrame.Value);
        activeFrame = variableSync.activeFrame.Value;

        if (variableSync.isRecordingLoaded.Value)
        {
            timelineSet = true;
            OnTimelineLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual void OnRecordingLoadedChanged(bool previous, bool current)
    {
        if (!current)
        {
            OnTimelineReset?.Invoke(this, EventArgs.Empty);
            timelineSet = false;
            maxValue = 1;
            minValue = 0;
            maxAccessibleValue = 1;
            minAccessibleValue = 0;
            activeFrame = 0;
        }

        if (current)
        {
            maxValue = variableSync.replayLength.Value;
            minValue = variableSync.minFrame.Value;
            maxAccessibleValue = variableSync.maxFrame.Value;
            minAccessibleValue = variableSync.minFrame.Value;
            activeFrame = variableSync.activeFrame.Value;

            timelineSet = true;
            OnTimelineLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    protected void OnReplayLengthChanged(int previous, int current)
    {
        if (!timelineSet) return;
        SetMaxValue(current);
    }

    protected virtual void OnMaxFrameChanged(int previous, int current)
    {
        if (!timelineSet) return;
        SetMaxAccessibleValue(current);
    }

    protected virtual void OnMinFrameChanged(int previous, int current)
    {
        if (!timelineSet) return;
        SetMinAccessibleValue(current);
    }

    protected virtual void SetMinValue(float value)
    {
        if (!timelineSet) return;
        if (value > maxAccessibleValue)
        {
            SetMaxAccessibleValue(value);
        }
        minValue = value;

        OnTimelineChanged?.Invoke(this, EventArgs.Empty);
    }
    protected virtual void SetMaxValue(float value)
    {
        if (!timelineSet) return;
        if (value < minAccessibleValue)
        {
            SetMinAccessibleValue(value);
        }
        maxValue = value;

        OnTimelineChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void SetMaxAccessibleValue(float value)
    {
        if (!timelineSet) return;
        if (value > maxValue)
        {
            value = maxValue;
        }
        maxAccessibleValue = value;
    }

    protected virtual void SetMinAccessibleValue(float value)
    {
        if (!timelineSet) return;
        if (value < minValue)
        {
            value = minValue;
        }
        minAccessibleValue = value;
    }

    public void ChangeActiveFrame(float frame)
    {
        if (!timelineSet) return;
        if (variableSync.RequestTimelineLock(NetworkManager.LocalClientId))
        {
            inUse = true;
            interactionCoordinator.Lock();
            variableSync.SetFrameServerRpc(frame);
            variableSync.FreeAccessServerRpc();
            inUse = false;
        }
    }

    public float GetMaxValue()
    {
        return maxValue;
    }

    public float GetMinValue()
    {
        return minValue;
    }

    public bool IsTimelineSet()
    {
        return timelineSet;
    }
}
