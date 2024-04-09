using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * gives references to data of loaded file
 */


public class ReplayManager : MonoBehaviour
{
    public static ReplayManager Instance { get; private set; }

    public event EventHandler<OnReplayLoadedEventArgs> OnReplayLoaded;
    public class OnReplayLoadedEventArgs
    {
        public RecordingSO replaySO;
    }

    public event EventHandler OnReplayUnloaded;

    public LogDataManager tutorialLogDataManager, taskOneLogDataManager, taskTwoLogDataManager, taskThreeLogDataManager;

    private LogDataManager activeLogDataManager;

    private int totalFrames;

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

    private bool isLoading = false;
    private bool fileLoaded = false;

    private RecordingSO activeReplaySO;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one ReplayManager found");
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

    public void Load(RecordingSO replaySO)
    {
        StartLoad(replaySO.savefile);

        activeReplaySO = replaySO;

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

        totalFrames = headTransformLogs.Count - 1;

        fileLoaded = true;

        OnReplayLoaded?.Invoke(this, new OnReplayLoadedEventArgs
        {
            replaySO = activeReplaySO
        });

    }

    public int GetReplayLength()
    {
        return totalFrames;
    }

    public void Unload()
    {
        fileLoaded = false;
        OnReplayUnloaded?.Invoke(this, EventArgs.Empty);
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

    public List<ArmLog> GetTopArmLogs() {  return topArmLogs; }
    public List<ArmLog> GetMiddleArmLogs() { return middleArmLogs; }

    public List<int> GetBottomArmHighlights(){ return bottomArmHighlights; }
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

    public RecordingSO GetActiveReplaySO() { return activeReplaySO; }
}