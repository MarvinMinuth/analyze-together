using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Trajectories : TimelineNew
{
    private enum BodyPart
    {
        Head,
        LeftHand,
        RightHand,
    }


    [SerializeField] private BodyPart bodyPart;
    [SerializeField] private GameObject ghostObject;
    [SerializeField] private MeshRenderer bodyMeshRenderer;
    private int minFrame, maxFrame;
    private List<TransformLog> transformLogs = new List<TransformLog>();
    [SerializeField] private LineRenderer lineRenderer;
    //private FighterLoader fighterLoader;

    [SerializeField] private Material opaqueTrajectoryMaterial;
    [SerializeField] private Material transparentTrajectoryMaterial;

    private bool isDragged = false;

    [SerializeField] private XRGrabInteractable grabInteractable;

    protected override void Initialize()
    {
        base.Initialize();

        /*
        fighterLoader = FighterLoader.Instance;
        if (fighterLoader == null)
        {
            Debug.LogError("No FighterLoader found");
        }
        */

        grabInteractable.enabled = false;

        if (FighterCoordinator.Instance != null)
        {
            ListenToFighterCoordinatorEvents();
        }
        else
        {
            NetworkServerSetup.Instance.OnServerSetupComplete += NetworkServerSetup_OnServerSetupComplete;
        }
    }

    private void ListenToFighterCoordinatorEvents()
    {
        switch (bodyPart)
        {
            case BodyPart.Head:
                FighterCoordinator.Instance.OnHeadHidden += FighterCoordinator_OnBodyPartHidden;
                FighterCoordinator.Instance.OnHeadShown += FighterCoordinator_OnBodyPartShown;
                break;
            case BodyPart.LeftHand:
                FighterCoordinator.Instance.OnLeftHandHidden += FighterCoordinator_OnBodyPartHidden;
                FighterCoordinator.Instance.OnLeftHandShown += FighterCoordinator_OnBodyPartShown;
                break;
            case BodyPart.RightHand:
                FighterCoordinator.Instance.OnRightHandHidden += FighterCoordinator_OnBodyPartHidden;
                FighterCoordinator.Instance.OnRightHandShown += FighterCoordinator_OnBodyPartShown;
                break;
        }
    }

    private void NetworkServerSetup_OnServerSetupComplete(object sender, System.EventArgs e)
    {
        ListenToFighterCoordinatorEvents();
    }

    protected override void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        DestroyTrajectories();
    }

    private void FighterCoordinator_OnBodyPartShown(object sender, System.EventArgs e)
    {
        if (minFrame == 0 && maxFrame == 0) return;
        grabInteractable.enabled = true;
        lineRenderer.enabled = true;
    }

    private void FighterCoordinator_OnBodyPartHidden(object sender, System.EventArgs e)
    {
        grabInteractable.enabled = false;
        lineRenderer.enabled = false;
    }

    private void ReplayController_OnReplayWindowUnset(object sender, System.EventArgs e)
    {
        DestroyTrajectories();
    }

    protected override void ReplayController_OnReplayControllerLoaded(object sender, EventArgs e)
    {
        switch (bodyPart)
        {
            case BodyPart.Head:
                transformLogs = replayController.GetRecordingData().GetHeadTransformLogs();
                break;
            case BodyPart.LeftHand:
                transformLogs = replayController.GetRecordingData().GetLeftHandTransformLogs();
                break;
            case BodyPart.RightHand:
                transformLogs = replayController.GetRecordingData().GetRightHandTransformLogs();
                break;
        }
    }

    protected override void ReplayController_OnReplayWindowSet(object sender, ReplayController.OnReplayWindowSetEventArgs e)
    {
        SetFrames(e.minReplayWindowFrame, e.maxReplayWindowFrame);
        CreateTrajectories();
    }

    void Update()
    {
        if (isDragged)
        {
            WhileDragged();
        }
    }

    public void SetFrames(int minFrame, int maxFrame)
    {
        this.minFrame = minFrame;
        this.maxFrame = maxFrame;
    }

    public void SetLogs(List<TransformLog> logs)
    {
        this.transformLogs = logs;
    }

    public void CreateTrajectories()
    {
        lineRenderer.positionCount = maxFrame - minFrame;
        for (int i = minFrame; i < maxFrame; i++)
        {
            lineRenderer.SetPosition(i - minFrame, transformLogs[i].Position);
        }
        lineRenderer.Simplify(0.05f);
        grabInteractable.enabled = true;
    }

    public void DestroyTrajectories()
    {
        //transformLogs.Clear();
        grabInteractable.enabled = false;
        lineRenderer.positionCount = 0;
        minFrame = 0;
        maxFrame = 0;
    }

    public void StartDrag()
    {
        if (interactionCoordinator.isInteractionInProgress.Value)
        {
            return;
        }

        inUse = true;

        FighterCoordinator.Instance.SetFighterMovement(false);

        interactionCoordinator.InitStartInteraction();

        // wasRunning = replayController.IsPlaying();
        replayController.InitPause();

        //fighterLoader.PreventLoading();
        ghostObject.SetActive(true);
        isDragged = true;
        if (bodyMeshRenderer != null)
        {
            bodyMeshRenderer.enabled = false;
        }
        lineRenderer.material = opaqueTrajectoryMaterial;
    }

    public void EndDrag()
    {
        if (!inUse)
        {
            return;
        }

        FighterCoordinator.Instance.SetFighterMovement(true);
        isDragged = false;
        int frame = CalculateFrameToLoad();
        replayController.InitSetFrame(frame);
        ghostObject.SetActive(false);
        //fighterLoader.AllowLoading();
        if (bodyMeshRenderer != null)
        {
            bodyMeshRenderer.enabled = true;
        }
        lineRenderer.material = transparentTrajectoryMaterial;


        inUse = false;
        interactionCoordinator.EndInteraction();
    }

    public void WhileDragged()
    {
        int frame = CalculateFrameToLoad();
        SetGhostObjectTransform(frame);
        replayController.InitSetFrame(frame);
    }

    private int CalculateFrameToLoad()
    {
        Vector3 position = grabInteractable.transform.position;
        int nearestFrame = 0;

        float nearestDistance = float.MaxValue;

        for (int i = minFrame; i <= maxFrame; i++)
        {
            Vector3 loggedPosition = transformLogs[i].Position;
            float distance = Vector3.Distance(position, loggedPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestFrame = i;
            }
        }
        return nearestFrame;
    }

    private void SetGhostObjectTransform(int frame)
    {
        ghostObject.transform.position = transformLogs[frame].Position;
        ghostObject.transform.eulerAngles = transformLogs[frame].Rotation;
    }
    protected override void InteractionCoordinator_OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (!inUse && current)
        {
            grabInteractable.enabled = false;
        }
        else
        {
            grabInteractable.enabled = true;
        }
    }

    private bool IsBodyPartShown()
    {
        switch (bodyPart)
        {
            case BodyPart.Head:
                return FighterCoordinator.Instance.IsHeadShown();
            case BodyPart.LeftHand:
                return FighterCoordinator.Instance.IsLeftHandShown();
            case BodyPart.RightHand:
                return FighterCoordinator.Instance.IsRightHandShown();
        }
        return false;
    }

    /*
    int FindNearestVector3Key(Vector3 target, int time, int positionsToCheck)
    {
        positionsToCheck = positionsToCheck / 2;
        int nearestKey = 0;
        float nearestDistance = float.MaxValue;

        for (int i = -positionsToCheck; i <= positionsToCheck; i++)
        {
            if (positions.ContainsKey(time + i))
            {
                float distance = Vector3.Distance(target, positions[time + i]);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestKey = time + i;
                }
            }
        }
        return nearestKey;
    }
    */
}
