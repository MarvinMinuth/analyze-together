using UnityEngine;
using UnityEngine.UI;

public class BackwardsButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ReplayController replayController;

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
