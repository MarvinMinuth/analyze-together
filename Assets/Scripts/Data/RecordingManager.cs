using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * gives references to data of loaded file
 * has basically 4 states: "No file loaded -> Loading -> File Loaded -> Unloading"
 */

// Savefile enums are used to easily match Logs and RecordingSOs
public enum Savefile
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
        public RecordingSO recordingSO;
    }

    public event EventHandler OnRecordingUnloaded;

    public LogDataManager tutorialLogDataManager, taskOneLogDataManager, taskTwoLogDataManager, taskThreeLogDataManager;

    private LogDataManager activeLogDataManager;

    private int maxFrame;

    private List<TransformLog> headTransformLogs;
    private List<TransformLog> leftHandTransformLogs;
    private List<TransformLog> rightHandTransformLogs;

    private List<ArmLog> bottomArmLogs;
    private List<ArmLog> middleArmLogs;
    private List<ArmLog> topArmLogs;

    private List<int> bottomArmHighlights;
    private List<int> middleArmHighlights;
    private List<int> topArmHighlights;
    private Dictionary<int, ArmCollisionLog[]> armCollisionLogDic;
    private Dictionary<int, ArmCollisionLog[]> succsessfulArmCollisionLogDic;
    private Dictionary<int, ArmCollisionLog[]> unsuccsessfulArmCollisionLogDic;
    private Dictionary<int, FightCollisionLog[]> fightCollisionLogDic;
    private Dictionary<int, FightCollisionLog[]> succsessfulFightCollisionLogDic;
    private Dictionary<int, FightCollisionLog[]> unsuccsessfulFightCollisionLogDic;

    private Dictionary<int, HRLog> hrLogDic;

    // the states don't need much logic, so we just use bools instead
    private bool isLoading = false;
    private bool fileLoaded = false;

    private RecordingSO activeRecordingSO;

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

    private void StartLoad(Savefile saveFile)
    {
        if (isLoading) return;

        isLoading = true;

        if (fileLoaded)
        {
            Unload();
        }

        switch (saveFile)
        {
            case Savefile.Tutorial:
                activeLogDataManager = tutorialLogDataManager;
                break;
            case Savefile.TaskOne:
                activeLogDataManager = taskOneLogDataManager;
                break;
            case Savefile.TaskTwo:
                activeLogDataManager = taskTwoLogDataManager;
                break;
            case Savefile.TaskThree:
                activeLogDataManager = taskThreeLogDataManager;
                break;
        }
    }

    public void Load(RecordingSO recordingSO)
    {
        StartLoad(recordingSO.savefile);

        activeRecordingSO = recordingSO;

        if (!activeLogDataManager.AreLogsReady() && !activeLogDataManager.IsLoading())
        {
            activeLogDataManager.LoadReplay();
        }

        StartCoroutine(WaitForLogs());
    }
    IEnumerator WaitForLogs()
    {
        while (!activeLogDataManager.AreLogsReady())
        {
            yield return null;
        }

        headTransformLogs = activeLogDataManager.GetHeadTransformLogs();
        leftHandTransformLogs = activeLogDataManager.GetLeftHandTransformLogs();
        rightHandTransformLogs = activeLogDataManager.GetRightHandTransformLogs();

        bottomArmLogs = activeLogDataManager.GetBottomArmLogs();
        middleArmLogs = activeLogDataManager.GetMiddleArmLogs();
        topArmLogs = activeLogDataManager.GetTopArmLogs();

        bottomArmHighlights = activeLogDataManager.GetBottomArmHighlights();
        middleArmHighlights = activeLogDataManager.GetMiddleArmHighlights();
        topArmHighlights = activeLogDataManager.GetTopArmHighlights();

        armCollisionLogDic = activeLogDataManager.GetArmCollisionLogs();
        succsessfulArmCollisionLogDic = activeLogDataManager.GetSuccsessfulArmCollisionLogs();
        unsuccsessfulArmCollisionLogDic = activeLogDataManager.GetUnsuccsessfulArmCollisionLogs();
        fightCollisionLogDic = activeLogDataManager.GetFightCollisionLogs();
        succsessfulFightCollisionLogDic = activeLogDataManager.GetSuccsessfulFightCollisionLogs();
        unsuccsessfulFightCollisionLogDic = activeLogDataManager.GetUnsuccsessfulFightCollisionLogs();
        hrLogDic = activeLogDataManager.GetHRLogs();

        OnLogsLoaded();
    }

    private void OnLogsLoaded()
    {
        isLoading = false;

        maxFrame = headTransformLogs.Count - 1;

        fileLoaded = true;

        OnRecordingLoaded?.Invoke(this, new OnRecordingLoadedEventArgs
        {
            recordingSO = activeRecordingSO
        });

    }

    public int GetMaxFrame()
    {
        return maxFrame;
    }

    public void Unload()
    {
        fileLoaded = false;

        headTransformLogs = null;
        leftHandTransformLogs = null;
        rightHandTransformLogs = null;

        bottomArmLogs = null;
        middleArmLogs = null;
        topArmLogs = null;

        bottomArmHighlights = null;
        middleArmHighlights = null;
        topArmHighlights = null;

        armCollisionLogDic = null;
        succsessfulArmCollisionLogDic = null;
        unsuccsessfulArmCollisionLogDic = null;
        fightCollisionLogDic = null;
        succsessfulFightCollisionLogDic = null;
        unsuccsessfulFightCollisionLogDic = null;
        hrLogDic = null;

        OnRecordingUnloaded?.Invoke(this, EventArgs.Empty);
    }

    /* In HR-Manager �berf�hren
    // Gib den HRLog zur�ck, der dem gegebenen Frame am n�chsten kommt
    public int GetCurrentHeartRate()
    {
        if (!fileLoaded || isLoading)
        {
            return 0;
        }
        int nearestFrame = -1;
        foreach (int key in hrLogDic.Keys)
        {
            // Wenn der Schl�ssel kleiner oder gleich dem gegebenen Frame ist
            if (key <= frame)
            {
                // Aktualisiere den Wert von nearestFrame, wenn der Schl�ssel gr��er ist
                if (key > nearestFrame)
                {
                    nearestFrame = key;
                }
            }
        }

        // Wenn nearestFrame aktualisiert wurde, gib den entsprechenden HRLog zur�ck
        if (nearestFrame != -1)
        {
            return hrLogDic[nearestFrame].heartRate;
        }

        // Wenn kein passender HRLog gefunden wurde, gib null zur�ck
        return 0;
    }
    */

    public List<ArmLog> GetBottomArmLogs()
    {
        return bottomArmLogs;
    }

    public List<ArmLog> GetTopArmLogs() { return topArmLogs; }
    public List<ArmLog> GetMiddleArmLogs() { return middleArmLogs; }

    public List<int> GetBottomArmHighlights() { return bottomArmHighlights; }
    public List<int> GetTopArmHighlights() { return topArmHighlights; }
    public List<int> GetMiddleArmHighlights() { return middleArmHighlights; }

    public Dictionary<int, ArmCollisionLog[]> GetArmCollisionDic() { return armCollisionLogDic; }

    public Dictionary<int, ArmCollisionLog[]> GetSuccsessfulArmCollisionDic() { return succsessfulArmCollisionLogDic; }
    public Dictionary<int, ArmCollisionLog[]> GetUnsuccsessfulArmCollisionDic() { return unsuccsessfulArmCollisionLogDic; }

    public Dictionary<int, FightCollisionLog[]> GetFightCollisionDic() { return fightCollisionLogDic; }
    public Dictionary<int, FightCollisionLog[]> GetSuccsessfulFightCollisionDic() { return succsessfulFightCollisionLogDic; }
    public Dictionary<int, FightCollisionLog[]> GetUnsuccsessfulFightCollisionDic() { return unsuccsessfulFightCollisionLogDic; }

    public Dictionary<int, HRLog> GetHRLog() { return hrLogDic; }

    public bool IsLoading() { return isLoading; }

    public bool FileIsLoaded()
    {
        return fileLoaded;
    }

    public List<TransformLog> GetHeadTransformLogs() { return headTransformLogs; }
    public List<TransformLog> GetLeftHandTransformLogs() { return leftHandTransformLogs; }
    public List<TransformLog> GetRightHandTransformLogs() { return rightHandTransformLogs; }

    public RecordingSO GetActiveReplaySO() { return activeRecordingSO; }

    public Dictionary<int, HRLog> GetHRLogs() { return hrLogDic; }
}