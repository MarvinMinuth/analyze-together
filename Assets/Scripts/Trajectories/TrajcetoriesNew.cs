using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum BodyPart
{
    Head,
    LeftHand,
    RightHand
}
[RequireComponent(typeof(LineRenderer))]
public class TrajcetoriesNew : MonoBehaviour
{
    [SerializeField] private BodyPart bodyPart;
    [SerializeField] private float simplificationRate = 0.25f;
    private LineRenderer lineRenderer;
    private ReplayController replayController;
    private int maxPlayFrame;
    private int minPlayFrame;

    private List<TransformLog> transformLogs;

    private Vector3 offset;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        replayController = ReplayController.Instance;

        replayController.OnReplayWindowActivated += ReplayController_OnReplayWindowActivated;
        replayController.OnReplayWindowSet += ReplayController_OnReplayWindowSet;
        replayController.OnReplayWindowReset += ReplayController_OnReplayWindowReset;
        replayController.OnReplayControllerUnload += ReplayController_OnReplayControllerUnload;

        offset = FighterCoordinator.Instance.offset;
    }

    private void ReplayController_OnReplayWindowActivated(object sender, System.EventArgs e)
    {
        Activate();
        GetTransformLogs();
        SetLineRendererPositions();
    }

    private void Activate()
    {
        minPlayFrame = replayController.GetMinPlayFrame();
        maxPlayFrame = replayController.GetMaxPlayFrame();
        lineRenderer.enabled = true;
    }

    private void GetTransformLogs()
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

    private void SetLineRendererPositions()
    {
        lineRenderer.positionCount = maxPlayFrame - minPlayFrame + 1;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, transformLogs[minPlayFrame + i].Position - offset);
        }

        lineRenderer.Simplify(simplificationRate);
    }

    private void ReplayController_OnReplayWindowSet(object sender, ReplayController.OnReplayWindowSetEventArgs e)
    {
        if (transformLogs == null)
        {
            GetTransformLogs();
        }
        DeleteLineRendererPositions();
        minPlayFrame = e.minReplayWindowFrame;
        maxPlayFrame = e.maxReplayWindowFrame;
        SetLineRendererPositions();
    }

    private void DeleteLineRendererPositions()
    {
        lineRenderer.positionCount = 0;
    }

    private void ReplayController_OnReplayWindowReset(object sender, System.EventArgs e)
    {
        Deactivate();
    }

    private void Deactivate()
    {
        DeleteLineRendererPositions();
        lineRenderer.enabled = false;
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        Deactivate();
        transformLogs = null;
    }
}
