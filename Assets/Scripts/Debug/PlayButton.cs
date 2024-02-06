using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    private ReplayController replayController;
    [SerializeField] private Button playButton;

    private void Start()
    {
        replayController = ReplayController.Instance;

        playButton.onClick.AddListener(replayController.Play);
    }
}
