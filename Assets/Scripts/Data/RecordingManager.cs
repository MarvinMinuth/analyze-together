using System;
using System.Collections;
using UnityEngine;

/*
 * gives references to data of loaded file
 * has basically 4 states: "No file loaded -> Loading -> File Loaded -> Unloading"
 */

// Savefile enums are used to easily match Logs and RecordingSOs
public enum SaveFile
{
    None,
    Tutorial,
    TaskOne,
    TaskTwo,
    TaskThree,
}

public class RecordingManager : MonoBehaviour
{
    public static RecordingManager Instance { get; private set; }

    public event EventHandler<OnRecordingLoadedEventArgs> OnRecordingLoaded;
    public class OnRecordingLoadedEventArgs
    {
        public RecordingData loadedRecordingData;
    }

    public event EventHandler OnRecordingUnload;

    [SerializeField] private LogDataManager tutorialLogDataManager, taskOneLogDataManager, taskTwoLogDataManager, taskThreeLogDataManager;

    private LogDataManager activeLogDataManager;

    // the states don't need much logic, so we just use bools instead
    private bool isLoading = false;
    private bool fileLoaded = false;

    private RecordingData recordingData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one RecordingManager found");
        }
    }

    public void Load(SaveFile saveFile)
    {
        if (isLoading) return;

        isLoading = true;

        if (fileLoaded)
        {
            Unload();
        }

        switch (saveFile)
        {
            case SaveFile.Tutorial:
                activeLogDataManager = tutorialLogDataManager;
                break;
            case SaveFile.TaskOne:
                activeLogDataManager = taskOneLogDataManager;
                break;
            case SaveFile.TaskTwo:
                activeLogDataManager = taskTwoLogDataManager;
                break;
            case SaveFile.TaskThree:
                activeLogDataManager = taskThreeLogDataManager;
                break;
        }

        if (!activeLogDataManager.AreLogsReady() && !activeLogDataManager.IsLoading())
        {
            activeLogDataManager.LoadReplay();
        }

        StartCoroutine(WaitForLogs(saveFile));
    }
    IEnumerator WaitForLogs(SaveFile saveFile)
    {
        while (!activeLogDataManager.AreLogsReady())
        {
            yield return null;
        }

        recordingData = new RecordingData(saveFile, activeLogDataManager.GetHeadTransformLogs(), activeLogDataManager.GetLeftHandTransformLogs(), activeLogDataManager.GetRightHandTransformLogs(), activeLogDataManager.GetBottomArmLogs(), activeLogDataManager.GetMiddleArmLogs(), activeLogDataManager.GetTopArmLogs(), activeLogDataManager.GetBottomArmHighlights(), activeLogDataManager.GetMiddleArmHighlights(), activeLogDataManager.GetTopArmHighlights(), activeLogDataManager.GetArmCollisionLogs(), activeLogDataManager.GetSuccsessfulArmCollisionLogs(), activeLogDataManager.GetUnsuccsessfulArmCollisionLogs(), activeLogDataManager.GetFightCollisionLogs(), activeLogDataManager.GetSuccsessfulFightCollisionLogs(), activeLogDataManager.GetUnsuccsessfulFightCollisionLogs(), activeLogDataManager.GetHRLogs());

        OnLogsLoaded();
    }

    private void OnLogsLoaded()
    {
        isLoading = false;

        fileLoaded = true;

        OnRecordingLoaded?.Invoke(this, new OnRecordingLoadedEventArgs
        {
            loadedRecordingData = recordingData
        });
    }

    public void Unload()
    {
        OnRecordingUnload?.Invoke(this, EventArgs.Empty);

        activeLogDataManager = null;
        recordingData = null;
    }
}