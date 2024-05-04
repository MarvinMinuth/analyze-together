using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ReplayController replayController;

    private InteractionCoordinator interactionCoordinator;

    private void Start()
    {
        replayController = ReplayController.Instance;

        if (!replayController.IsPlaying())
        {
            SetButtonActive();
        }
        else
        {
            SetButtonInactive();
        }

        replayController.OnPause += OnPause;
        replayController.OnPlay += OnPlay;
        replayController.OnReplayControllerUnload += OnPause;

        button.onClick.AddListener(replayController.InitPause);

        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
    }

    private void OnPause(object sender, System.EventArgs e)
    {
        SetButtonActive();
    }

    private void OnPlay(object sender, System.EventArgs e)
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

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        button.interactable = !current;
    }
}
