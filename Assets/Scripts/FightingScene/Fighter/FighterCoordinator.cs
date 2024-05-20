using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class FighterCoordinator : NetworkBehaviour
{
    public static FighterCoordinator Instance { get; private set; }
    public event EventHandler OnFighterInPosition;

    public event EventHandler OnFighterInitialized;
    public event EventHandler OnHeadHidden;
    public event EventHandler OnHeadShown;
    public event EventHandler OnLeftHandHidden;
    public event EventHandler OnLeftHandShown;
    public event EventHandler OnRightHandHidden;
    public event EventHandler OnRightHandShown;
    public event EventHandler OnBodyHidden;
    public event EventHandler OnBodyShown;
    private ReplayController replayController;

    [SerializeField] private Material idleMaterial;
    [SerializeField] private Material hmdMaterial;

    [SerializeField] private FighterVisuals fighterVisuals;
    [SerializeField] private Transform headTransform, leftHandTransform, rightHandTransform;
    [SerializeField] private Trajectories headTrajectories, leftHandTrajectories, rightHandTrajectories;
    private bool headShown, leftHandShown, rightHandShown, bodyShown;
    [SerializeField] private LoadingStatueSO taskOneLoadingStatueSO, tutorialLoadingStatueSO, taskTwoLoadingStatueSO, taskthreeLoadingStatueSO;
    private LoadingStatueSO loadingStatueSO;
    private bool fighterMovementEnabled = true;

    private List<TransformLog> headTransformLogs;
    private List<TransformLog> rightHandTransformLogs;
    private List<TransformLog> leftHandTransformLogs;

    [SerializeField] private Transform offsetTransform;
    [SerializeField] private Transform fightingSceneTransform;
    public Vector3 offset;

    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one FighterCoordinator found");
        }
    }
    private void Start()
    {
        if (IsServer)
        {
            replayController = ReplayController.Instance;

            replayController.OnReplayDataReady += ReplayController_OnReplayDataReady;
            replayController.OnActiveFrameChanged += ReplayController_OnFrameChanged;
            replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;

            FighterLoader.Instance.OnFighterInPosition += FighterLoader_OnFighterInPosition;
            fighterVisuals.Hide();

            IsInitialized = true;
            OnFighterInitialized?.Invoke(this, EventArgs.Empty);
        }

        offset = offsetTransform.localPosition - fightingSceneTransform.position;
    }

    private void ReplayController_OnReplayControllerUnload(object sender, EventArgs e)
    {
        headTransformLogs = null;
        rightHandTransformLogs = null;
        leftHandTransformLogs = null;
        ChangeToIdleMaterial();
        fighterVisuals.Hide();
    }

    private void ReplayController_OnFrameChanged(object sender, ReplayController.OnActiveFrameChangedEventArgs e)
    {
        if (!fighterMovementEnabled) return;
        //transform.position = new Vector3(e.headTransformLog.Position.x, 0, e.headTransformLog.Position.z);
        int newFrame = e.newActiveFrame;

        headTransform.position = headTransformLogs[e.newActiveFrame].Position - offset;
        headTransform.rotation = Quaternion.Euler(headTransformLogs[e.newActiveFrame].Rotation);

        leftHandTransform.position = leftHandTransformLogs[newFrame].Position - offset;
        leftHandTransform.rotation = Quaternion.Euler(leftHandTransformLogs[newFrame].Rotation);

        rightHandTransform.position = rightHandTransformLogs[newFrame].Position - offset;
        rightHandTransform.rotation = Quaternion.Euler(rightHandTransformLogs[newFrame].Rotation);

        fighterVisuals.ResetVisuals();
    }

    private void ReplayController_OnReplayDataReady(object sender, EventArgs e)
    {

        headTransformLogs = replayController.GetRecordingData().GetHeadTransformLogs();
        leftHandTransformLogs = replayController.GetRecordingData().GetLeftHandTransformLogs();
        rightHandTransformLogs = replayController.GetRecordingData().GetRightHandTransformLogs();

        switch (replayController.GetRecordingData().GetSaveFile())
        {
            case SaveFile.TaskOne:
                loadingStatueSO = taskOneLoadingStatueSO;
                break;
            case SaveFile.TaskTwo:
                loadingStatueSO = taskTwoLoadingStatueSO;
                break;
            case SaveFile.Tutorial:
                loadingStatueSO = tutorialLoadingStatueSO;
                break;
            case SaveFile.TaskThree:
                loadingStatueSO = taskthreeLoadingStatueSO;
                break;
        }

        //fighterVisuals.Show();

    }

    public List<MeshRenderer> GetVisualMeshRenderers()
    {
        return fighterVisuals.GetAllMeshRenderers();
    }


    private void FighterLoader_OnFighterInPosition(object sender, EventArgs e)
    {
        ShowHead();
        ShowLeftHand();
        ShowRightHand();
        ShowBody();
        fighterVisuals.Show();
        fighterVisuals.ChangeMaterial(loadingStatueSO.material, false);

        OnFighterInPosition?.Invoke(this, EventArgs.Empty);
    }



    public void SetHeadTransparent()
    {
        headShown = false;
        fighterVisuals.ChangeHeadMaterial(loadingStatueSO.transparentMaterial, true);

        OnHeadHidden?.Invoke(this, EventArgs.Empty);
    }

    public void ShowHead()
    {
        headShown = true;
        fighterVisuals.ChangeHeadMaterial(loadingStatueSO.material, false);
        fighterVisuals.ChangeHMDMaterial(loadingStatueSO.hmdMaterial);

        OnHeadShown?.Invoke(this, EventArgs.Empty);
    }

    public void SetLeftHandTransparent()
    {
        leftHandShown = false;
        fighterVisuals.ChangeLeftHandMaterial(loadingStatueSO.transparentMaterial);

        OnLeftHandHidden?.Invoke(this, EventArgs.Empty);
    }

    public void ShowLeftHand()
    {
        leftHandShown = true;
        fighterVisuals.ChangeLeftHandMaterial(loadingStatueSO.material);

        OnLeftHandShown?.Invoke(this, EventArgs.Empty);
    }

    public void SetRightHandTransparent()
    {
        rightHandShown = false;
        fighterVisuals.ChangeRightHandMaterial(loadingStatueSO.transparentMaterial);

        OnRightHandHidden?.Invoke(this, EventArgs.Empty);
    }

    public void ShowRightHand()
    {
        rightHandShown = true;
        fighterVisuals.ChangeRightHandMaterial(loadingStatueSO.material);

        OnRightHandShown?.Invoke(this, EventArgs.Empty);
    }

    public void SetBodyTransparent()
    {
        bodyShown = false;
        fighterVisuals.ChangeBodyMaterial(loadingStatueSO.transparentMaterial);

        OnBodyHidden?.Invoke(this, EventArgs.Empty);
    }

    public void ShowBody()
    {
        bodyShown = true;
        fighterVisuals.ChangeBodyMaterial(loadingStatueSO.material);

        OnBodyShown?.Invoke(this, EventArgs.Empty);
    }


    /*
    private void ReplayManager_OnReplayUnloaded(object sender, System.EventArgs e)
    {
        if (headTrajectories != null) { headTrajectories.DestroyTrajectories(); }
        if (leftHandTrajectories != null) { leftHandTrajectories.DestroyTrajectories(); }
        if (rightHandTrajectories != null) { rightHandTrajectories.DestroyTrajectories(); }

        ChangeToIdleMaterial();
        fighterVisuals.Hide();
    }
    */

    public void ChangeToIdleMaterial()
    {
        fighterVisuals.ChangeMaterial(idleMaterial, false);
        fighterVisuals.ChangeHMDMaterial(hmdMaterial);
    }

    public bool IsFighterMovementEnabled()
    {
        return fighterMovementEnabled;
    }

    public void SetFighterMovement(bool enable)
    {
        fighterMovementEnabled = enable;
    }


    public bool IsHeadShown()
    {
        return headShown;
    }
    public bool IsLeftHandShown()
    {
        return leftHandShown;
    }
    public bool IsRightHandShown()
    {
        return rightHandShown;
    }
    public bool IsBodyShown()
    {
        return bodyShown;
    }

}
