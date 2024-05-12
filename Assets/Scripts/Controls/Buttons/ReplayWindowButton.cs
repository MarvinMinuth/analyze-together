using UnityEngine;
using UnityEngine.UI;

public class ReplayWindowButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ReplayController replayController;

    private InteractionCoordinator interactionCoordinator;

    private void Start()
    {
        replayController = ReplayController.Instance;

        if (replayController.IsReplayWindowActive)
        {
            SetButtonActive();
        }
        else
        {
            SetButtonInactive();
        }

        replayController.OnReplayWindowActivated += OnReplayWindowActivated;
        replayController.OnReplayWindowReset += OnReplayWindowReset;

        button.onClick.AddListener(ToggleReplayWindow);

        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        button.interactable = !current;
    }

    private void OnReplayWindowActivated(object sender, System.EventArgs e)
    {
        SetButtonActive();
    }

    private void OnReplayWindowReset(object sender, System.EventArgs e)
    {
        SetButtonInactive();
    }

    public void SetButtonActive()
    {
        button.GetComponent<Image>().color = Color.grey;
    }

    public void SetButtonInactive()
    {
        button.GetComponent<Image>().color = Color.white;
    }

    private void ToggleReplayWindow()
    {
        if (replayController.IsReplayWindowActive)
        {
            replayController.InitResetReplayWindow();
        }
        else
        {
            replayController.InitActivateReplayWindow();
        }
    }
}
