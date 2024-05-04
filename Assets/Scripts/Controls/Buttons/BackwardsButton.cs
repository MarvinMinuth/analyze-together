using UnityEngine;
using UnityEngine.UI;

public class BackwardsButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ReplayController replayController;

    private InteractionCoordinator interactionCoordinator;

    private void Start()
    {
        replayController = ReplayController.Instance;

        if (replayController.GetDirection() == Direction.Backward)
        {
            SetButtonActive();
        }
        else
        {
            SetButtonInactive();
        }

        replayController.OnDirectionChanged += OnReplayDirectionChanged;

        button.onClick.AddListener(replayController.InitChangeDirection);

        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        button.interactable = !current;
    }

    private void OnReplayDirectionChanged(object sender, ReplayController.OnDirectionChangedEventArgs e)
    {
        if (e.direction == Direction.Backward)
        {
            SetButtonActive();
        }
        else
        {
            SetButtonInactive();
        }
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
