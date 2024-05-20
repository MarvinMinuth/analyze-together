using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.XR.Interaction.Toolkit;
using System.Linq;

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
    [SerializeField] private FighterBodyPartVisuals fighterBodyPartVisuals;

    public event EventHandler OnTrajectoryActivated;
    public event EventHandler OnTrajectoryDeactivated;
    public event EventHandler<OnPositionChangedEventArgs> OnPositionsChanged;
    public class OnPositionChangedEventArgs : EventArgs
    {
        public Dictionary<int, Vector3> positionsWithIndices;
    }

    public event EventHandler OnPositionsDeleted;
    private LineRenderer lineRenderer;
    private ReplayController replayController;
    private int maxPlayFrame;
    private int minPlayFrame;

    private List<TransformLog> transformLogs;

    private Vector3 offset;

    public bool IsActivated { get { return lineRenderer.enabled; } }

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

        fighterBodyPartVisuals.OnVisibilityChanged += FighterBodyPartVisuals_OnVisibilityChanged;

        offset = FighterCoordinator.Instance.offset;

        if (fighterBodyPartVisuals.IsVisible() && replayController.IsReplayWindowActive)
        {
            Activate();
            GetTransformLogs();
            SetLineRendererPositions();
        }
        else
        {
            Deactivate();
        }
    }

    private void FighterBodyPartVisuals_OnVisibilityChanged(object sender, System.EventArgs e)
    {
        if (fighterBodyPartVisuals.IsVisible() && replayController.IsReplayWindowActive)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
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

        OnTrajectoryActivated?.Invoke(this, EventArgs.Empty);
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

        Vector3[] newPositions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(newPositions);

        List<int> keptIndices = new List<int>();

        LineUtility.Simplify(newPositions.ToList(), simplificationRate, keptIndices);

        List<Vector3> simplifiedPositionsList = new List<Vector3>();
        foreach (int index in keptIndices)
        {
            simplifiedPositionsList.Add(newPositions[index]);
        }

        lineRenderer.positionCount = simplifiedPositionsList.Count;
        lineRenderer.SetPositions(simplifiedPositionsList.ToArray());

        Dictionary<int, Vector3> simplifiedPositionsDict = new Dictionary<int, Vector3>();

        for (int i = 0; i < simplifiedPositionsList.Count; i++)
        {
            simplifiedPositionsDict.Add(keptIndices[i] + minPlayFrame, simplifiedPositionsList[i]);
        }

        OnPositionsChanged?.Invoke(this, new OnPositionChangedEventArgs { positionsWithIndices = simplifiedPositionsDict });
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

        OnTrajectoryDeactivated?.Invoke(this, EventArgs.Empty);
    }

    private void ReplayController_OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        Deactivate();

        OnPositionsDeleted?.Invoke(this, EventArgs.Empty);
        transformLogs = null;
    }
}
