using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MinSlider : MonoBehaviour
{
    [SerializeField] private ReplayWindowController replayWindowController;
    [SerializeField] private Slider minSlider;
    private ReplayController replayController;
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
            minSlider.interactable = !current;
        }
    }

    public void StartDrag()
    {
        if (minSlider.interactable == false)
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
        if (!inUse || minSlider.interactable == false)
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
        if (!inUse || moreThanOneInteractor)
        {
            minSlider.value = replayWindowController.MinPlayFrame;
            return;
        }

        float min = minSlider.value;
        float max = replayController.GetMaxPlayFrame();

        if (max - min < replayWindowController.GetMinDistance())
        {
            minSlider.value = max - replayWindowController.GetMinDistance();
        }

        ChangeReplayWindow();
    }
}
