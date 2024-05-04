using UnityEngine;
using UnityEngine.UI;

public class LoopButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ReplayController replayController;

    private InteractionCoordinator interactionCoordinator;

    private void Start()
    {
        replayController = ReplayController.Instance;

        if (replayController.IsLooping())
        {
            SetButtonActive();
        }
        else
        {
            SetButtonInactive();
        }

        replayController.OnRepeat += OnRepeat;

        button.onClick.AddListener(replayController.InitChangeLooping);

        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        button.interactable = !current;
    }

    private void OnRepeat(object sender, System.EventArgs e)
    {
        if (replayController.IsLooping())
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
