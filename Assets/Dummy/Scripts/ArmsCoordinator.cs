using System.Collections.Generic;
using UnityEngine;

/*
 * Gets logData from ReplayManager
 * Listens to ReplayController and sets arms for active frame
 */
public class ArmsCoordinator : MonoBehaviour
{
    public string prefabFolder = "FightingDummy/Prefabs";

    public GameObject bottomArmBase;
    public GameObject middleArmBase;
    public GameObject topArmBase;
    private List<GameObject> armBases;

    private Dictionary<GameObject, GameObject> activeArms = new Dictionary<GameObject, GameObject>();

    private ReplayController replayController;
    private ReplayManager replayManager;

    private List<ArmLog> bottomArmLogs;
    private List<ArmLog> middleArmLogs;
    private List<ArmLog> topArmLogs;

    [SerializeField] private Transform offsetTransform;
    [SerializeField] private Transform fightingSceneTransform;
    private Vector3 offset;

    void Start()
    {
        offset = offsetTransform.localPosition - fightingSceneTransform.position;

        replayController = ReplayController.Instance;
        replayManager = ReplayManager.Instance;

        replayController.OnReplayDataReady += ReplayController_OnReplayDataReady;
        replayController.OnFrameChanged += ReplayController_OnFrameChanged;
        replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;

        armBases = new List<GameObject>
        {
            bottomArmBase,
            middleArmBase,
            topArmBase
        };
    }

    private void ReplayController_OnReplayDataReady(object sender, System.EventArgs e)
    {
        bottomArmLogs = replayManager.GetBottomArmLogs();
        middleArmLogs = replayManager.GetMiddleArmLogs();
        topArmLogs = replayManager.GetTopArmLogs();
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        bottomArmLogs = null;
        middleArmLogs = null;
        topArmLogs = null;
        ResetArms();
    }

    private void ReplayController_OnFrameChanged(object sender, ReplayController.OnFrameChangedEventArgs e)
    {
        SetArmBases(e.frame);
    }

    private void SetArmBases(int frame)
    {
        SetBottomArmBase(frame);
        SetMiddleArmBase(frame);
        SetTopArmBase(frame);
    }
    private void SetBottomArmBase(int frame)
    {
        ArmLog activeLog = bottomArmLogs[frame];
        AttachToArmBase(bottomArmBase, activeLog.armType);
        SetPosition(bottomArmBase, activeLog.Position - offset);
        SetRotation(bottomArmBase, Quaternion.Euler(activeLog.Rotation));
        SetTargetPosition(bottomArmBase, activeLog.targetPosition - offset);
        SetTargetRotation(bottomArmBase, Quaternion.Euler(activeLog.targetRotation));
    }

    private void SetMiddleArmBase(int frame)
    {
        ArmLog activeLog = middleArmLogs[frame];
        AttachToArmBase(middleArmBase, activeLog.armType);
        SetPosition(middleArmBase, activeLog.Position - offset);
        SetRotation(middleArmBase, Quaternion.Euler(activeLog.Rotation));
        SetTargetPosition(middleArmBase, activeLog.targetPosition - offset);
        SetTargetRotation(middleArmBase, Quaternion.Euler(activeLog.targetRotation));
    }

    private void SetTopArmBase(int frame)
    {
        ArmLog activeLog = topArmLogs[frame];
        AttachToArmBase(topArmBase, activeLog.armType);
        SetPosition(topArmBase, activeLog.Position - offset);
        SetRotation(topArmBase, Quaternion.Euler(activeLog.Rotation));
        SetTargetPosition(topArmBase, activeLog.targetPosition - offset);
        SetTargetRotation(topArmBase, Quaternion.Euler(activeLog.targetRotation));
    }

    GameObject FindArmSlot(GameObject armBase)
    {
        return armBase.transform.Find("Binding").Find("ArmSlot").gameObject;
    }

    public void SetPosition(GameObject armBase, Vector3 position)
    {
        if (armBase == null)
        {
            return;
        }
        armBase.transform.position = position;
    }

    public void SetRotation(GameObject armBase, Quaternion rotation)
    {
        if (armBase == null)
        {
            return;
        }
        armBase.transform.rotation = rotation;
    }

    public void SetTargetPosition(GameObject armBase, Vector3 targetPosition)
    {
        Transform target = CheckForTarget(armBase);
        if (target == null)
        {
            return;
        }
        target.transform.position = targetPosition;
    }

    public void SetTargetRotation(GameObject armBase, Quaternion rotation)
    {
        Transform target = CheckForTarget(armBase);
        if (target == null)
        {
            return;
        }
        target.transform.rotation = rotation;
    }

    private Transform CheckForTarget(GameObject armBase)
    {
        if (armBase == null || !activeArms.ContainsKey(armBase) || activeArms[armBase] == null)
        {
            return null;
        }
        Transform target = activeArms[armBase].transform.Find("Target");

        return target;
    }

    public void AttachToArmBase(GameObject armBase, string arm)
    {
        GameObject armSlot = FindArmSlot(armBase);
        GameObject spawnArm = armSlot.transform.Find(arm).gameObject;
        if (spawnArm == null || (activeArms.ContainsKey(armBase) && activeArms[armBase] == spawnArm))
        {
            return;   
        }
        DetachFromArmBase(armBase);
        spawnArm.SetActive(true);
        activeArms.Add(armBase, spawnArm);
    }

    public void DetachFromArmBase(GameObject armBase)
    {
        if (!activeArms.ContainsKey(armBase))
        {
            return;
        }
        activeArms[armBase].SetActive(false);
        activeArms.Remove(armBase);
    }
    public void DetachFromBottom()
    {
        DetachFromArmBase(bottomArmBase);
    }

    public void DetachFromMiddle()
    {
        DetachFromArmBase(middleArmBase);
    }

    public void DetachFromTop()
    {
        DetachFromArmBase(topArmBase);
    }

    public void DetachAll()
    {
        DetachFromBottom();
        DetachFromMiddle();
        DetachFromTop();
    }

    public void ResetArms()
    {
        DetachAll();
        foreach(GameObject armBase in armBases)
        {
            SetPosition(armBase, new Vector3(0, 0, 0));
            SetRotation(armBase, new Quaternion(0, 0, 0, 0));
        }
    }
}
