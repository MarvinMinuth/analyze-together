using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ReplayController replayController;

    private InteractionCoordinator interactionCoordinator;

    private void Start()
    {
        replayController = ReplayController.Instance;

        if (replayController.IsPlaying())
        {
            SetButtonActive();
        }
        else
        {
            SetButtonInactive();
        }

        replayController.OnPlay += OnPlay;
        replayController.OnPause += OnPause;
        replayController.OnReplayControllerUnload += OnPause;

        button.onClick.AddListener(replayController.InitPlay);

        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        button.interactable = !current;
    }

    private void OnPlay(object sender, System.EventArgs e)
    {
        SetButtonActive();
    }

    private void OnPause(object sender, System.EventArgs e)
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
}
