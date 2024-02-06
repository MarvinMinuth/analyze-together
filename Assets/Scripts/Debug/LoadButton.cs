using UnityEngine;
using UnityEngine.UI;

public class LoadButton : MonoBehaviour
{
    private ReplayController replayController;
    [SerializeField] private ReplaySO replaySO;
    [SerializeField] private Button loadButton;

    private void Start()
    {
        replayController = ReplayController.Instance;
        loadButton.onClick.AddListener(LoadReplay);
    }

    private void LoadReplay()
    {
        replayController.Load(replaySO);
    }

}
