using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FighterLoader : MonoBehaviour
{
    public static FighterLoader Instance { get; private set; }

    public event EventHandler OnFighterInPosition;
    public event EventHandler OnReplayFinished;
    public event EventHandler OnAllReplaysFinished;

    [Header("General")]
    [SerializeField] private Material defaultMaterial;
    //[SerializeField] private TaskWall taskWall;
    [SerializeField] private float transitionTime = 5f;

    [Header("Tutorial Statue")]
    [SerializeField] private FighterLoadingStatue tutorialLoadingStatue;
    [SerializeField] private bool tutorialEnabled;

    [Header("Task One Statue")]
    [SerializeField] private FighterLoadingStatue taskOneLoadingStatue;
    [SerializeField] private bool taskOneEnabled;

    [Header("Task Two Statue")]
    [SerializeField] private FighterLoadingStatue taskTwoLoadingStatue;
    [SerializeField] private bool taskTwoEnabled;

    [Header("Task Three Statue")]
    [SerializeField] private FighterLoadingStatue taskThreeLoadingStatue;
    [SerializeField] private bool taskThreeEnabled;

    private ReplayController replayController;
    private FighterLoadingStatue loadedFighter;
    private FighterLoadingStatue statueShowingMessage;

    private int finishedCounter = 0;

    private bool loadingAllowed = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one FighterLoader found!");
        }
    }

    void Start()
    {
        replayController = ReplayController.Instance;
        if (replayController == null)
        {
            Debug.LogError("No ReplayController found");
        }

        replayController.OnReplayControllerLoaded += ReplayController_OnReplayControllerLoaded;

        if (!NetworkServerSetup.Instance.ServerIsSetup)
        {
            HideStatues();
            NetworkServerSetup.Instance.OnServerSetupComplete += NetworkServerSetup_OnServerSetupComplete;
        }
        else
        {
            Debug.Log("Server already setup");
        }
    }

    private void NetworkServerSetup_OnServerSetupComplete(object sender, EventArgs e)
    {
        ShowStatues();
    }

    private void ReplayController_OnReplayControllerLoaded(object sender, EventArgs e)
    {
        loadingAllowed = false;
        loadedFighter.HideMessage();
        loadedFighter.InteractionsAllowed(false);
        loadedFighter.MoveLoadingStatueToStartPosition(transitionTime);
        //loadedFighter.HideStatue();
        loadedFighter.DisableInteractable();
    }

    public void InvokeOnFighterInPosition()
    {
        loadedFighter.HideStatue();
        loadedFighter.ResetLoadingStatueFighter();
        loadingAllowed = true;
        OnFighterInPosition?.Invoke(this, EventArgs.Empty);
    }

    public void LoadReplay(SaveFile saveFile)
    {
        if (!loadingAllowed) return;

        // Reset currently loaded statue
        if (loadedFighter != null && !loadedFighter.IsFinished())
        {
            loadedFighter.ShowStatue();
            loadedFighter.ActivateInteractable();
            loadedFighter = null;
        }

        switch (saveFile)
        {
            case SaveFile.Tutorial:
                loadedFighter = tutorialLoadingStatue;
                break;
            case SaveFile.TaskOne:
                loadedFighter = taskOneLoadingStatue;
                break;
            case SaveFile.TaskTwo:
                loadedFighter = taskTwoLoadingStatue;
                break;
            case SaveFile.TaskThree:
                loadedFighter = taskThreeLoadingStatue;
                break;
        }

        replayController.InitLoad(loadedFighter.GetLoadingStatueSO().saveFile);
    }

    public void FinishLoadedReplay()
    {
        if (loadedFighter == null) return;
        loadedFighter.Finish();

        OnReplayFinished?.Invoke(this, EventArgs.Empty);

        loadedFighter = null;
        replayController.InitUnload();

        finishedCounter++;

        if (finishedCounter == 4)
        {
            OnAllReplaysFinished?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ShowMessage(FighterLoadingStatue fighterLoadingStatue)
    {
        if (statueShowingMessage != null)
        {
            statueShowingMessage.HideMessage();
        }
        statueShowingMessage = fighterLoadingStatue;
        fighterLoadingStatue.ShowMessage();
    }

    public void HideMessage()
    {
        statueShowingMessage = null;
    }

    public void PreventLoading()
    {
        //if (tutorialLoadingStatue != null) { tutorialLoadingStatue.InteractionsAllowed(false);  tutorialLoadingStatue.HideMessage(); }
        //if (taskOneLoadingStatue != null) { taskOneLoadingStatue.InteractionsAllowed(false); taskOneLoadingStatue.HideMessage(); }
        //if (taskTwoLoadingStatue != null) { taskTwoLoadingStatue.InteractionsAllowed(false); taskTwoLoadingStatue.HideMessage(); }
        //if (taskThreeLoadingStatue != null) { taskThreeLoadingStatue.InteractionsAllowed(false); taskThreeLoadingStatue.HideMessage(); }
    }

    public void AllowLoading()
    {
        if (tutorialLoadingStatue != null && !tutorialLoadingStatue.IsFinished()) { tutorialLoadingStatue.InteractionsAllowed(true); }
        if (taskOneLoadingStatue != null && !taskOneLoadingStatue.IsFinished()) { taskOneLoadingStatue.InteractionsAllowed(true); }
        if (taskTwoLoadingStatue != null && !taskTwoLoadingStatue.IsFinished()) { taskTwoLoadingStatue.InteractionsAllowed(true); }
        if (taskThreeLoadingStatue != null && !taskThreeLoadingStatue.IsFinished()) { taskThreeLoadingStatue.InteractionsAllowed(true); }
    }

    public void HideStatues()
    {
        if (tutorialLoadingStatue != null) { tutorialLoadingStatue.HideStatue(); }
        if (taskOneLoadingStatue != null) { taskOneLoadingStatue.HideStatue(); }
        if (taskTwoLoadingStatue != null) { taskTwoLoadingStatue.HideStatue(); }
        if (taskThreeLoadingStatue != null) { taskThreeLoadingStatue.HideStatue(); }
    }

    public void ShowStatues()
    {
        if (tutorialLoadingStatue != null) { tutorialLoadingStatue.ShowStatue(); }
        if (taskOneLoadingStatue != null) { taskOneLoadingStatue.ShowStatue(); }
        if (taskTwoLoadingStatue != null) { taskTwoLoadingStatue.ShowStatue(); }
        if (taskThreeLoadingStatue != null) { taskThreeLoadingStatue.ShowStatue(); }
    }

}
