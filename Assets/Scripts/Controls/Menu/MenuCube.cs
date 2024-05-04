using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MenuCube : MonoBehaviour
{
    public Transform target;
    public float distanceToPlace = 1.0f;
    public bool IsDragged { get; private set; } = false;

    private InteractionCoordinator interactionCoordinator;
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private MenuPositionNetworkVariables menuPositionNetworkVariables;

    [SerializeField] private bool isSynced = true;

    // Start is called before the first frame update

    private void Initialize()
    {
        AlignToTarget();

        interactionCoordinator = InteractionCoordinator.Instance;
        interactionCoordinator.isInteractionInProgress.OnValueChanged += OnInteractionInProgressChanged;
    }

    private void Start()
    {
        Initialize();
    }

    private void OnInteractionInProgressChanged(bool previous, bool current)
    {
        if (IsDragged || !isSynced)
        {
            return;
        }
        grabInteractable.enabled = !current;
    }

    // Update is called once per frame
    private void Update()
    {
        if (IsDragged)
        {
            AlignToTarget();
        }
    }

    public void OnSelectExit()
    {
        if (!IsDragged)
        {
            return;
        }
        IsDragged = false;
        AlignToTarget();
        interactionCoordinator.EndInteraction();
    }

    public void OnSelectEnter()
    {
        IsDragged = true;
        interactionCoordinator.InitStartInteraction();
    }

    public void AlignToTarget()
    {
        // Stelle sicher, dass das Ziel-GameObject existiert
        if (target != null)
        {
            // Richte das Hauptobjekt in Richtung des Ziel-GameObjects aus
            transform.LookAt(target);

            // Setze die X- und Z-Rotation auf 0 und behalte die Y-Rotation bei
            Vector3 currentRotation = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, currentRotation.y, 0);
            transform.position.Set(transform.position.x, 0, transform.position.z);

            if (isSynced)
            {
                menuPositionNetworkVariables.SetPosition(transform.position, transform.rotation);
            }
        }
    }

    public void PlaceInFrontOfTarget()
    {
        transform.position.Set(target.transform.position.x, target.transform.position.y, target.transform.position.z + 1);
        AlignToTarget();
    }

    public void SetPosition(Vector3 position, Quaternion rotation)
    {
        if (IsDragged || !isSynced)
        {
            return;
        }
        transform.position = position;
        transform.rotation = rotation;
    }

    public void SetTarget(Transform newTarget)
    {
        if (!isSynced) return;

        target = newTarget;
        AlignToTarget();
    }
}
