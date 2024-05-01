using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class StopButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ReplayController replayController;
    private InteractionCoordinator interactionCoordinator;



    private void Start()
    {
        replayController = ReplayController.Instance;

        button.onClick.AddListener(replayController.InitStop);

        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        button.interactable = !current;
    }
}
