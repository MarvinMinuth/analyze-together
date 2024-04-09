using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastController : MonoBehaviour
{
    public GameObject leftRayInteractor, rightRayInteractor;
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
            rightRayInteracting = true;
        }
        else
        {
            rightRayInteracting = false;
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
