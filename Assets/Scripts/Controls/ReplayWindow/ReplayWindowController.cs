using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReplayWindowController : MonoBehaviour
{
    [SerializeField] private Slider maxSlider;
    [SerializeField] private Slider minSlider;
    [SerializeField] private float minDistance = 30;

    [SerializeField] private bool maxSliderFollowsMin = true;

    private ReplayController replayController;

    public float MaxPlayFrame { get; private set; }
    public float MinPlayFrame { get; private set; }

    private void Start()
    {
        replayController = ReplayController.Instance;

        replayController.OnReplayControllerLoaded += ReplayController_OnReplayControllerLoaded;
        replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;
        replayController.OnReplayWindowSet += ReplayController_OnReplayWindowSet;
        replayController.OnReplayWindowReset += ReplayController_OnReplayWindowReset;

        if (replayController.IsInitialized)
        {
            maxSlider.maxValue = replayController.GetMaxFrame();
            MaxPlayFrame = replayController.GetMaxPlayFrame();
            minSlider.maxValue = replayController.GetMaxFrame();
            MinPlayFrame = replayController.GetMinPlayFrame();
            minSlider.value = MinPlayFrame;

            if (maxSliderFollowsMin)
            {
                maxSlider.value = minSlider.value + minDistance;
            }
            else
            {
                maxSlider.value = MaxPlayFrame;
            }
        }
        else
        {
            maxSlider.maxValue = 1;
            MaxPlayFrame = 1;
            maxSlider.value = 1;
            minSlider.maxValue = 1;
            MinPlayFrame = 0;
            minSlider.value = 0;
        }

    }

    private void ReplayController_OnReplayControllerLoaded(object sender, System.EventArgs e)
    {
        maxSlider.maxValue = replayController.GetMaxFrame();
        MaxPlayFrame = replayController.GetMaxPlayFrame();
        minSlider.maxValue = replayController.GetMaxFrame();
        MinPlayFrame = 0;
        minSlider.value = 0;

        if (maxSliderFollowsMin)
        {
            maxSlider.value = minSlider.value + minDistance;
        }
        else
        {
            maxSlider.value = MaxPlayFrame;
        }
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        maxSlider.maxValue = 1;
        maxSlider.value = 1;
        MaxPlayFrame = 1;
        minSlider.maxValue = 1;
        MinPlayFrame = 0;
        minSlider.value = 0;
    }

    private void ReplayController_OnReplayWindowSet(object sender, ReplayController.OnReplayWindowSetEventArgs e)
    {
        MinPlayFrame = e.minReplayWindowFrame;
        minSlider.value = e.minReplayWindowFrame;

        if (maxSliderFollowsMin)
        {
            MaxPlayFrame = e.minReplayWindowFrame + minDistance;
            maxSlider.value = e.minReplayWindowFrame + minDistance;
        }
        else
        {
            MaxPlayFrame = e.maxReplayWindowFrame;
            maxSlider.value = e.maxReplayWindowFrame;
        }
    }

    private void ReplayController_OnReplayWindowReset(object sender, System.EventArgs e)
    {
        MinPlayFrame = 0;
        minSlider.value = 0;

        if (maxSliderFollowsMin)
        {
            MaxPlayFrame = minDistance;
            maxSlider.value = minDistance;
        }
        else
        {
            MaxPlayFrame = replayController.GetMaxFrame();
            maxSlider.value = replayController.GetMaxFrame();
        }
    }

    public void ChangeReplayWindow()
    {
        if (maxSliderFollowsMin)
        {
            maxSlider.value = minSlider.value + minDistance;
        }
        ReplayController.Instance.InitChangeReplayWindow((int)minSlider.value, (int)maxSlider.value);
    }

    public void ChangeReplayWindow(int min, int max)
    {
        ReplayController.Instance.InitChangeReplayWindow(min, max);
    }

    public float GetMinDistance()
    {
        return minDistance;
    }
}
