using UnityEngine;
using UnityEngine.UI;

public class LoadButton : MonoBehaviour
{
    private ReplayController replayController;
    [SerializeField] private RecordingSO replaySO;
    [SerializeField] private Button loadButton;

    public void Setup()
    {
        replayController = ReplayController.Instance;
        loadButton.onClick.AddListener(LoadReplay);
    }

    private void LoadReplay()
    {
        replayController.InitLoad(replaySO.saveFile);
    }

}
