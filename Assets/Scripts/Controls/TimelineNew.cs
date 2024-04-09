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
    public event EventHandler OnTimelineChanged;
    protected int activeFrame;
    protected float minValue, maxValue;
    protected float minAccessibleValue, maxAccessibleValue;
    protected InteractionCoordinator interactionCoordinator;

    private ReplayController replayController;
    protected NetworkVariableSync variableSync;

    protected bool inUse;


    private void Start()
    {
        interactionCoordinator = InteractionCoordinator.Instance;

        inUse = false;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        variableSync = NetworkVariableSync.Instance;

        variableSync.replayLength.OnValueChanged += OnReplayLengthChanged;
        variableSync.minFrame.OnValueChanged += OnMinFrameChanged;
        variableSync.maxFrame.OnValueChanged += OnMaxFrameChanged;

        SetMaxValue(variableSync.replayLength.Value);
        SetMinValue(variableSync.minFrame.Value);
        SetMaxAccessibleValue(variableSync.maxFrame.Value);
        SetMinAccessibleValue(variableSync.minFrame.Value);
        activeFrame = variableSync.activeFrame.Value;

        OnTimelineChanged?.Invoke(this, EventArgs.Empty);
    }

    protected void OnReplayLengthChanged(int previous, int current)
    {
        if (current <= 1) { SetMaxValue(1); } //if a replay is Unloaded maxValue is set to 1
        else { SetMaxValue(current - 1); }
    }

    protected virtual void OnMaxFrameChanged(int previous, int current)
    {
        SetMaxAccessibleValue(current);
    }

    protected virtual void OnMinFrameChanged(int previous, int current)
    {
        SetMinAccessibleValue(current);
    }

    protected virtual void SetMinValue(float value)
    {
        if (value > maxAccessibleValue)
        {
            SetMaxAccessibleValue(value);
        }
        minValue = value;

        OnTimelineChanged?.Invoke(this, EventArgs.Empty);
    }
    protected virtual void SetMaxValue(float value)
    {
        value = value - 1;
        if (value < minAccessibleValue)
        {
            SetMinAccessibleValue(value);
        }
        maxValue = value;

        OnTimelineChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void SetMaxAccessibleValue(float value)
    {
        if (value > maxValue)
        {
            value = maxValue;
        }
        maxAccessibleValue = value;
    }

    protected virtual void SetMinAccessibleValue(float value)
    {
        if (value < minValue)
        {
            value = minValue;
        }
        minAccessibleValue = value;
    }

    public void ChangeActiveFrame(float frame)
    {
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
}
