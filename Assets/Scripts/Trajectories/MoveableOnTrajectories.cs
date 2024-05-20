using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class MoveableOnTrajectories : MonoBehaviour
{
    [SerializeField] private TrajcetoriesNew trajectoriesNew;
    [SerializeField] private FighterBodyPartVisuals fighterBodyPartVisuals;
    [SerializeField] private MoveableObjectSync moveableObjectSync;
    private ReplayController replayController;

    private InteractionCoordinator interactionCoordinator;
    private XRGrabInteractable grabInteractable;
    public bool IsDragged { get; private set; } = false;
    private bool wasRunning = false;

    private Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>();
    private Vector3 offset;

    public bool IsHidden { get { return !GetComponent<FighterBodyPartVisuals>().IsVisible(); } }


    private void Start()
    {
        trajectoriesNew.OnTrajectoryActivated += TrajectoriesNew_OnTrajectoryActivated;
        trajectoriesNew.OnTrajectoryDeactivated += TrajectoriesNew_OnTrajectoryDeactivated;
        trajectoriesNew.OnPositionsChanged += TrajectoriesNew_OnPositionsChanged;
        trajectoriesNew.OnPositionsDeleted += TrajectoriesNew_OnPositionsDeleted;

        replayController = ReplayController.Instance;

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.enabled = false;

        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += InteractionCoordinator_OnInteractionInProgressChanged;

        if (fighterBodyPartVisuals.IsVisible() && trajectoriesNew.IsActivated)
        {
            grabInteractable.enabled = !interactionCoordinator.isInteractionInProgress.Value;
        }
        else
        {
            grabInteractable.enabled = false;
        }

        GetComponent<FighterBodyPartVisuals>().Hide();
    }

    private void InteractionCoordinator_OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (!IsDragged)
        {
            grabInteractable.enabled = !current;
        }
    }

    private void TrajectoriesNew_OnPositionsDeleted(object sender, System.EventArgs e)
    {
        positions = null;
    }

    private void TrajectoriesNew_OnPositionsChanged(object sender, TrajcetoriesNew.OnPositionChangedEventArgs e)
    {
        positions = e.positionsWithIndices;
    }

    private void TrajectoriesNew_OnTrajectoryActivated(object sender, System.EventArgs e)
    {
        grabInteractable.enabled = !interactionCoordinator.isInteractionInProgress.Value;
    }

    private void TrajectoriesNew_OnTrajectoryDeactivated(object sender, System.EventArgs e)
    {
        grabInteractable.enabled = false;
    }

    void Update()
    {
        if (IsDragged)
        {
            WhileDragged();
        }
    }

    public void StartDrag()
    {
        if (interactionCoordinator.isInteractionInProgress.Value)
        {
            return;
        }

        // FighterCoordinator.Instance.SetFighterMovement(false);
        IsDragged = true;

        moveableObjectSync.ChangeHidden(false);
        interactionCoordinator.InitStartInteraction();

        wasRunning = replayController.IsPlaying();
        replayController.InitPause();

        //fighterLoader.PreventLoading();
        //ghostObject.SetActive(true);

        /*
        if (bodyMeshRenderer != null)
        {
            bodyMeshRenderer.enabled = false;
        }
        lineRenderer.material = opaqueTrajectoryMaterial;
        */
    }

    public void EndDrag()
    {
        if (!IsDragged)
        {
            return;
        }

        //FighterCoordinator.Instance.SetFighterMovement(true);

        int frame = CalculateFrameToLoad();
        replayController.InitSetFrame(frame);
        //transform.position = transformLogs[frame].Position - offset;
        //transform.rotation = Quaternion.Euler(transformLogs[frame].Rotation);

        /*
        ghostObject.SetActive(false);
        //fighterLoader.AllowLoading();
        if (bodyMeshRenderer != null)   
        {
            bodyMeshRenderer.enabled = true;
        }
        lineRenderer.material = transparentTrajectoryMaterial;
        */

        IsDragged = false;
        moveableObjectSync.ChangeHidden(true);
        interactionCoordinator.EndInteraction();
    }

    public void WhileDragged()
    {
        int frame = CalculateFrameToLoad();
        //SetGhostObjectTransform(frame);
        replayController.InitSetFrame(frame);
    }

    private int CalculateFrameToLoad()
    {
        Vector3 position = grabInteractable.transform.position;
        int nearestFrame = 0;

        float nearestDistance = float.MaxValue;

        foreach (KeyValuePair<int, Vector3> entry in positions)
        {
            int frameToTest = entry.Key;
            Vector3 positionToTest = entry.Value;

            float distance = Vector3.Distance(position, positionToTest);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestFrame = frameToTest;
            }
        }
        moveableObjectSync.SetPosition(transform.position, transform.rotation);
        return nearestFrame;
    }

    public void SetPosition(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }

    public void ChangeHidden(bool hidden)
    {
        if (hidden)
        {
            GetComponent<FighterBodyPartVisuals>().Hide();
            GetComponent<ParentConstraint>().constraintActive = true;
        }
        else
        {
            GetComponent<FighterBodyPartVisuals>().Show();
            GetComponent<ParentConstraint>().constraintActive = false;
        }
    }
}
