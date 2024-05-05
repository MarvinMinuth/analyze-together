using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class RaycastController : MonoBehaviour
{
    public GameObject leftRayInteractor, rightRayInteractor;
    [SerializeField] private RaycastListener rightRaycastListener;
    [SerializeField] private ActionBasedControllerManager teleportControllerManager;
    [SerializeField] private ActionBasedSnapTurnProvider snapTurnProvider;
    public InputActionReference leftActionReference, rightActionReference;
    public List<InputActionReference> leftBlockingActions, rightBlockingActions;

    private bool leftRayActive, rightRayActive;
    private bool leftRayInteracting, rightRayInteracting;

    void Start()
    {
        leftActionReference.action.performed += ChangeLeftRay;
        rightActionReference.action.performed += ChangeRightRay;

        foreach (InputActionReference action in leftBlockingActions)
        {
            action.action.performed += LeftRayInteracting;
            action.action.canceled += LeftRayInteracting;
        }

        foreach (InputActionReference action in rightBlockingActions)
        {
            action.action.performed += RightRayInteracting;
            action.action.canceled += RightRayInteracting;
        }

        leftRayInteractor.SetActive(false);
        rightRayInteractor.SetActive(false);

        rightRaycastListener.OnRaycastEnabled += OnRightRaycastEnabled;
    }

    private void OnTeleport(InputAction.CallbackContext context)
    {
        Debug.Log(context.action.name + " was triggered");
    }

    private void OnRightRaycastEnabled(object sender, System.EventArgs e)
    {
        if (!rightRayActive) { rightRayInteractor.SetActive(false); }
    }

    public void LeftRayInteracting(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            leftRayInteracting = true;
        }
        else
        {
            leftRayInteracting = false;
        }
    }

    public void RightRayInteracting(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            teleportControllerManager.enabled = false;
            snapTurnProvider.enabled = false;
            rightRayInteracting = true;
        }
        else
        {
            rightRayInteracting = false;
            snapTurnProvider.enabled = true;
            teleportControllerManager.enabled = true;
        }
    }

    public void ChangeLeftRay(InputAction.CallbackContext context)
    {
        if (leftRayInteracting) { return; }
        if (leftRayActive)
        {
            leftRayActive = false;
            leftRayInteractor.SetActive(false);
        }
        else
        {
            leftRayActive = true;
            leftRayInteractor.SetActive(true);
        }
    }

    public void ChangeRightRay(InputAction.CallbackContext context)
    {
        if (rightRayInteracting) { return; }
        if (rightRayActive)
        {
            rightRayActive = false;
            rightRayInteractor.SetActive(false);
        }
        else
        {
            rightRayActive = true;
            rightRayInteractor.SetActive(true);
        }
    }
}
