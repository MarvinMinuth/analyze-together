using UnityEngine;
using UnityEngine.UI;

public class DebugPlayButton : MonoBehaviour
{
    private ReplayController replayController;
    [SerializeField] private Button playButton;

    public void Setup()
    {
        replayController = ReplayController.Instance;

        playButton.onClick.AddListener(replayController.Play);
    }
}
