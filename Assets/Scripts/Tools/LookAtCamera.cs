using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class LookAtCamera : MonoBehaviour
{
    public enum Mode
    {
        LookAt,
        LookAtInverted,
        LookAtWithoutY,
        LookAtWithoutYInverted,
        CameraForward,
        CameraForwardInverted
    }

    [SerializeField] private Mode mode;
    private void LateUpdate()
    {
        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.LookAtInverted:
                Vector3 dirFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + dirFromCamera);
                break;
            case Mode.LookAtWithoutY:
                transform.LookAt(Camera.main.transform);
                Vector3 currentRotation = transform.eulerAngles;
                transform.eulerAngles = new Vector3(0, currentRotation.y, 0);
                transform.position.Set(transform.position.x, 0, transform.position.z);
                break;
            case Mode.LookAtWithoutYInverted:
                dirFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + dirFromCamera);
                currentRotation = transform.eulerAngles;
                transform.eulerAngles = new Vector3(0, currentRotation.y, 0);
                transform.position.Set(transform.position.x, 0, transform.position.z);
                break;
            case Mode.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            case Mode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }
    }
}

