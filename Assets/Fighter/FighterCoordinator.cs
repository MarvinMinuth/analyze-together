using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

public class FighterCoordinator : NetworkBehaviour
{
    public static FighterCoordinator Instance {  get; private set; }

    private RecordingManager replayManager;
    private ReplayController replayController;

    [SerializeField] private Material idleMaterial;
    [SerializeField] private Material hmdMaterial;

    [SerializeField] private FighterVisuals fighterVisuals;
    [SerializeField] private Transform headTransform, leftHandTransform, rightHandTransform;

    private RecordingSO loadedReplaySO;
    private bool fighterMovementEnabled = true;

    private List<TransformLog> headTransformLogs;
    private List<TransformLog> rightHandTransformLogs;
    private List<TransformLog> leftHandTransformLogs;

    [SerializeField] private Transform offsetTransform;
    [SerializeField] private Transform fightingSceneTransform;
    private Vector3 offset;

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
            replayManager = RecordingManager.Instance;

            replayController.OnReplayDataReady += ReplayController_OnReplayDataReady;
            replayController.OnFrameChanged += ReplayController_OnFrameChanged;
            replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;

            //FighterLoader.Instance.OnFighterInPosition += FighterLoader_OnFighterInPosition;
            fighterVisuals.Hide();
        }

        offset = offsetTransform.localPosition - fightingSceneTransform.position;
    }

    private void ReplayController_OnReplayControllerUnload(object sender, EventArgs e)
    {
        headTransformLogs = null;
        rightHandTransformLogs = null;
        leftHandTransformLogs = null;
        loadedReplaySO = null;
        ChangeToIdleMaterial();
        fighterVisuals.Hide();
    }

    private void ReplayController_OnFrameChanged(object sender, ReplayController.OnFrameChangedEventArgs e)
    {
        if (!fighterMovementEnabled) return;
        //transform.position = new Vector3(e.headTransformLog.Position.x, 0, e.headTransformLog.Position.z);
        int newFrame = e.frame;

        headTransform.position = headTransformLogs[e.frame].Position - offset;
        headTransform.rotation = Quaternion.Euler(headTransformLogs[e.frame].Rotation);

        if (newFrame >= leftHandTransformLogs.Count){
            newFrame = leftHandTransformLogs.Count - 1;
        }
        if ( newFrame < 0){
            newFrame = 0;
        }
        leftHandTransform.position = leftHandTransformLogs[newFrame].Position - offset;
        leftHandTransform.rotation = Quaternion.Euler(leftHandTransformLogs[newFrame].Rotation);

        if (newFrame >= rightHandTransformLogs.Count){
            newFrame = rightHandTransformLogs.Count - 1;
        }
        if ( newFrame < 0){
            newFrame = 0;
        }
        rightHandTransform.position = rightHandTransformLogs[newFrame].Position - offset;
        rightHandTransform.rotation = Quaternion.Euler(rightHandTransformLogs[newFrame].Rotation);

        //fighterVisuals.ResetVisuals();
    }

    private void ReplayController_OnReplayDataReady(object sender, EventArgs e)
    {
        loadedReplaySO = replayManager.GetActiveReplaySO();

        headTransformLogs = replayManager.GetHeadTransformLogs();
        leftHandTransformLogs = replayManager.GetLeftHandTransformLogs();
        rightHandTransformLogs = replayManager.GetRightHandTransformLogs();

        fighterVisuals.Show();

    }

    /*
    private void FighterLoader_OnFighterInPosition(object sender, EventArgs e)
    {
        ShowHead();
        ShowLeftHand();
        ShowRightHand();
        ShowBody();
        fighterVisuals.Show();
        //fighterVisuals.ChangeMaterial(loadingStatueSO.material, false);
    }
    */

    /*
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
    */

   
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

    /*
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
    */
}
