using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
 * Gets logData from ReplayManager
 * Listens to ReplayController and sets arms for active frame
 */
public class ArmsCoordinator : NetworkBehaviour
{
    [Header("Bottom Arm")]
    [SerializeField] private GameObject bottomArmBase;
    [SerializeField] private GameObject bottomArmSphere;
    [SerializeField] private List<Transform> bottomArmAttachements;

    [Header("Mid Arm")]
    [SerializeField] private GameObject middleArmBase;
    [SerializeField] private GameObject middleArmSphere;
    [SerializeField] private List<Transform> middleArmAttachements;

    [Header("Top Arm")]
    [SerializeField] private GameObject topArmBase;
    [SerializeField] private GameObject topArmSphere;
    [SerializeField] private List<Transform> topArmAttachements;

    private List<GameObject> armBases;

    private Dictionary<GameObject, GameObject> activeArms = new Dictionary<GameObject, GameObject>();

    private ReplayController replayController;
    private List<ArmLog> bottomArmLogs;
    private List<ArmLog> middleArmLogs;
    private List<ArmLog> topArmLogs;

    [Header("Offset")]
    [SerializeField] private Transform offsetTransform;
    [SerializeField] private Transform fightingSceneTransform;
    private Vector3 offset;

    void Start()
    {


        offset = offsetTransform.localPosition - fightingSceneTransform.position;

        armBases = new List<GameObject>
        {
            bottomArmBase,
            middleArmBase,
            topArmBase
        };

        if (IsServer)
        {
            replayController = ReplayController.Instance;

            replayController.OnReplayDataReady += ReplayController_OnReplayDataReady;
            replayController.OnActiveFrameChanged += ReplayController_OnFrameChanged;
            replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;

            ResetArms();
        }
    }

    private void ReplayController_OnReplayDataReady(object sender, System.EventArgs e)
    {
        bottomArmLogs = replayController.GetRecordingData().GetBottomArmLogs();
        middleArmLogs = replayController.GetRecordingData().GetMiddleArmLogs();
        topArmLogs = replayController.GetRecordingData().GetTopArmLogs();
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        bottomArmLogs = null;
        middleArmLogs = null;
        topArmLogs = null;
        ResetArms();
    }

    private void ReplayController_OnFrameChanged(object sender, ReplayController.OnActiveFrameChangedEventArgs e)
    {
        SetArmBases(e.newActiveFrame);
    }

    private void SetArmBases(int frame)
    {
        SetBottomArmBase(frame);
        SetMiddleArmBase(frame);
        SetTopArmBase(frame);
    }
    private void SetBottomArmBase(int frame)
    {
        if (frame > bottomArmLogs.Count - 1)
        {
            frame = bottomArmLogs.Count - 1;
        }
        if (frame < 0)
        {
            frame = 0;
        }
        ArmLog activeLog = bottomArmLogs[frame];
        AttachToArmBase(bottomArmBase, activeLog.armType);
        SetPosition(bottomArmBase, activeLog.Position - offset);
        SetRotation(bottomArmBase, Quaternion.Euler(activeLog.Rotation));
        SetTargetPosition(bottomArmBase, activeLog.targetPosition - offset);
        SetTargetRotation(bottomArmBase, Quaternion.Euler(activeLog.targetRotation));
    }

    private void SetMiddleArmBase(int frame)
    {
        if (frame > middleArmLogs.Count - 1)
        {
            frame = middleArmLogs.Count - 1;
        }
        if (frame < 0)
        {
            frame = 0;
        }
        ArmLog activeLog = middleArmLogs[frame];
        AttachToArmBase(middleArmBase, activeLog.armType);
        SetPosition(middleArmBase, activeLog.Position - offset);
        SetRotation(middleArmBase, Quaternion.Euler(activeLog.Rotation));
        SetTargetPosition(middleArmBase, activeLog.targetPosition - offset);
        SetTargetRotation(middleArmBase, Quaternion.Euler(activeLog.targetRotation));
    }

    private void SetTopArmBase(int frame)
    {
        if (frame > topArmLogs.Count - 1)
        {
            frame = topArmLogs.Count - 1;
        }
        if (frame < 0)
        {
            frame = 0;
        }
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
        spawnArm.GetComponent<DummyArmVisuals>().Show();
        activeArms.Add(armBase, spawnArm);
    }

    public void DetachFromArmBase(GameObject armBase)
    {
        if (!activeArms.ContainsKey(armBase))
        {
            return;
        }
        activeArms[armBase].GetComponent<DummyArmVisuals>().Hide();
        middleArmSphere.GetComponent<DummyArmVisuals>().Show();
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

        foreach (GameObject armBase in armBases)
        {
            //armBase.transform.localPosition = Vector3.zero;
            armBase.transform.localRotation = Quaternion.identity;
        }


        foreach (Transform armAttachement in bottomArmAttachements)
        {
            armAttachement.GetComponent<DummyArmVisuals>().Hide();
        }


        foreach (Transform armAttachement in middleArmAttachements)
        {
            armAttachement.GetComponent<DummyArmVisuals>().Hide();
        }

        foreach (Transform armAttachement in topArmAttachements)
        {
            armAttachement.GetComponent<DummyArmVisuals>().Hide();
        }

        bottomArmSphere.GetComponent<DummyArmVisuals>().Show();
        middleArmSphere.GetComponent<DummyArmVisuals>().Show();
        topArmSphere.GetComponent<DummyArmVisuals>().Show();
    }
}
