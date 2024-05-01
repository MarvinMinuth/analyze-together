using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MaxSlider : MonoBehaviour
{
    [SerializeField] private ReplayWindowController replayWindowController;
    [SerializeField] private Slider maxSlider;

    ReplayController replayController;
    private InteractionCoordinator interactionCoordinator;
    private bool moreThanOneInteractor;
    private bool wasRunning;
    private bool inUse;

    private void Start()
    {
        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;

        replayController = ReplayController.Instance;

        inUse = false;
    }

    public void ChangeReplayWindow()
    {
        replayWindowController.ChangeReplayWindow();
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (!inUse)
        {
            maxSlider.interactable = !current;
        }
    }

    public void StartDrag()
    {
        if (maxSlider.interactable == false)
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
        if (!inUse || maxSlider.interactable == false)
        {
            return;
        }
        if (moreThanOneInteractor)
        {
            moreThanOneInteractor = false;
            return;
        }

        inUse = false;
        interactionCoordinator.EndInteraction();

        if (wasRunning)
        {
            replayController.InitPlay();
        }
    }

    public void OnValueChanged()
    {
        if (!inUse)
        {
            maxSlider.value = replayWindowController.MaxPlayFrame;
            return;
        }

        float min = replayController.GetMinPlayFrame();
        float max = maxSlider.value;

        if (max - min < replayWindowController.GetMinDistance())
        {
            maxSlider.value = min + replayWindowController.GetMinDistance();
        }

        ChangeReplayWindow();
    }
}
