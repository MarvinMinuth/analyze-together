using UnityEngine;
using UnityEngine.UI;


/*
* Uses minReplayFrame to set value of slider
*/
public class WindowSlider : MonoBehaviour
{
    [SerializeField] private Slider windowSlider;
    [SerializeField] private RectTransform handleRect;
    private ReplayController replayController;
    private InteractionCoordinator interactionCoordinator;
    private bool moreThanOneInteractor;
    private bool wasRunning;
    private bool inUse;

    public float MaxPlayFrame { get; private set; }
    public float MinPlayFrame { get; private set; }

    private void Start()
    {
        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;

        replayController = ReplayController.Instance;

        replayController.OnReplayWindowActivated += ReplayController_OnReplayWindowActivated;
        replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;
        replayController.OnReplayWindowSet += ReplayController_OnReplayWindowSet;
        replayController.OnReplayWindowReset += ReplayController_OnReplayWindowReset;

        inUse = false;

        if (replayController.IsReplayWindowActive)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    private void ReplayController_OnReplayWindowActivated(object sender, System.EventArgs e)
    {
        Activate();
    }

    private void SetHandleSize()
    {
        float width = gameObject.GetComponent<RectTransform>().rect.width;
        handleRect.sizeDelta = new Vector2(width / replayController.GetMaxFrame() * replayController.replayWindowLength, handleRect.sizeDelta.y);
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        Deactivate();
    }

    private void ReplayController_OnReplayWindowSet(object sender, ReplayController.OnReplayWindowSetEventArgs e)
    {
        windowSlider.value = e.minReplayWindowFrame;
        MinPlayFrame = windowSlider.value;
        MaxPlayFrame = e.maxReplayWindowFrame;
    }

    private void ReplayController_OnReplayWindowReset(object sender, System.EventArgs e)
    {
        Deactivate();
    }

    public void ChangeReplayWindow()
    {
        ReplayController.Instance.InitChangeReplayWindow((int)windowSlider.value, (int)windowSlider.value + replayController.replayWindowLength);
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (!inUse)
        {
            windowSlider.interactable = !current;
        }
    }

    public void StartDrag()
    {
        if (windowSlider.interactable == false)
        {
            return;
        }
        if (inUse)
        {
            moreThanOneInteractor = true;
            return;
        }

        inUse = true;

        interactionCoordinator.InitStartInteraction();

        wasRunning = replayController.IsPlaying();
        replayController.InitPause();
    }

    public void EndDrag()
    {
        if (!inUse || windowSlider.interactable == false)
        {
            return;
        }
        if (moreThanOneInteractor)
        {
            moreThanOneInteractor = false;
            return;
        }

        inUse = false;
        interactionCoordinator.EndInteraction();

        if (wasRunning)
        {
            replayController.InitPlay();
        }
    }

    public void OnValueChanged()
    {
        if (!inUse || moreThanOneInteractor)
        {
            windowSlider.value = MinPlayFrame;
            return;
        }

        if (windowSlider.value > replayController.GetMaxFrame() - replayController.replayWindowLength)
        {
            windowSlider.value = replayController.GetMaxFrame() - replayController.replayWindowLength;
        }
        ChangeReplayWindow();
    }

    private void Activate()
    {
        if (replayController.IsInitialized)
        {
            windowSlider.maxValue = replayController.GetMaxFrame();
            MaxPlayFrame = replayController.GetMaxPlayFrame();
            MinPlayFrame = replayController.GetMinPlayFrame();
            windowSlider.value = replayController.GetActiveFrame();

            handleRect.gameObject.SetActive(true);
            SetHandleSize();

            ChangeReplayWindow();
        }
    }

    private void Deactivate()
    {
        windowSlider.value = 0;
        windowSlider.maxValue = 1;
        MaxPlayFrame = 1;
        MinPlayFrame = 0;

        handleRect.gameObject.SetActive(false);
    }
}
