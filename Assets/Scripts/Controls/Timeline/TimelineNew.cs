using System;
using UnityEngine;
/*
 * Every Timeline is a visualization of the ReplayController
 * When the network spawns the Timeline syncs with the relevant values
 */
public class TimelineNew : MonoBehaviour
{
    public event EventHandler OnTimelineLoaded;
    public event EventHandler OnTimelineChanged;
    public event EventHandler OnTimelineReset;
    protected int activeFrame;
    protected float minValue, maxValue;
    protected float minAccessibleValue, maxAccessibleValue;
    protected InteractionCoordinator interactionCoordinator;
    protected ReplayController replayController;

    protected bool inUse;

    public bool IsInitialized { get; private set; }

    private void Awake()
    {
        IsInitialized = false;
    }
    private void Start()
    {
        Initialize();
    }

    protected virtual void Initialize()
    {
        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += InteractionCoordinator_OnInteractionInProgressChanged;

        inUse = false;

        replayController = ReplayController.Instance;
        replayController.OnActiveFrameChanged += ReplayController_OnActiveFrameChanged;
        replayController.OnReplayWindowSet += ReplayController_OnReplayWindowSet;
        replayController.OnReplayWindowReset += ReplayController_OnReplayWindowReset;
        replayController.OnReplayControllerLoaded += ReplayController_OnReplayControllerLoaded;
        replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;

        if (replayController.IsInitialized)
        {
            InitializeTimelineValues(0, replayController.GetMaxFrame(), replayController.GetMinPlayFrame(), replayController.GetMaxPlayFrame(), replayController.GetActiveFrame());
            OnTimelineLoaded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            ResetTimelineValues();
        }
    }

    protected virtual void InteractionCoordinator_OnInteractionInProgressChanged(bool previous, bool current)
    {
    }

    protected virtual void ReplayController_OnReplayControllerUnload(object sender, EventArgs e)
    {
        OnTimelineReset?.Invoke(this, EventArgs.Empty);
        ResetTimelineValues();
    }

    private void ReplayController_OnReplayWindowSet(object sender, ReplayController.OnReplayWindowSetEventArgs e)
    {
        SetMinAccessibleValue(e.minReplayWindowFrame);
        SetMaxAccessibleValue(e.maxReplayWindowFrame);
    }

    protected virtual void ReplayController_OnActiveFrameChanged(object sender, ReplayController.OnActiveFrameChangedEventArgs e)
    {
        activeFrame = e.newActiveFrame;
    }

    protected virtual void ReplayController_OnReplayWindowReset(object sender, EventArgs e)
    {
        SetMinValue(0);
        SetMinAccessibleValue(0);
        SetMaxValue(replayController.GetMaxFrame());
        SetMaxAccessibleValue(replayController.GetMaxFrame());
    }

    protected virtual void ReplayController_OnReplayControllerLoaded(object sender, EventArgs e)
    {
        InitializeTimelineValues(0, replayController.GetMaxFrame(), replayController.GetMinPlayFrame(), replayController.GetMaxPlayFrame(), replayController.GetActiveFrame());
        OnTimelineLoaded?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void ResetTimelineValues()
    {
        this.minValue = 0;
        this.maxValue = 1;
        this.minAccessibleValue = 0;
        this.maxAccessibleValue = 1;
        this.activeFrame = 0;
    }

    protected virtual void InitializeTimelineValues(int minValue, int maxValue, int minAccessibleValue, int maxAccessibleValue, int activeFrame)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.minAccessibleValue = minAccessibleValue;
        this.maxAccessibleValue = maxAccessibleValue;
        this.activeFrame = activeFrame;

        IsInitialized = true;
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
    public float GetMaxValue()
    {
        return maxValue;
    }

    public float GetMinValue()
    {
        return minValue;
    }
}
