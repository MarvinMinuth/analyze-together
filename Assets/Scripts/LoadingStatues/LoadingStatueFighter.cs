using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingStatueFighter : MonoBehaviour
{
    [SerializeField] private Transform headTransform, leftHandTransform, rightHandTransform;
    [SerializeField] private Transform fighterVisuals;

    private Vector3 initialHeadPosition, initialLeftHandPosition, initialRightHandPosition;
    private Vector3 initialHeadRotation, initialLeftHandRotation, initialRightHandRotation;

    private void Awake()
    {
        initialHeadPosition = headTransform.position;
        initialLeftHandPosition = leftHandTransform.position;
        initialRightHandPosition = rightHandTransform.position;

        initialHeadRotation = headTransform.rotation.eulerAngles;
        initialLeftHandRotation = leftHandTransform.rotation.eulerAngles;
        initialRightHandRotation = rightHandTransform.rotation.eulerAngles;
    }

    public void MoveHeadToTargetLocation(Vector3 targetPosition, float time)
    {
        Vector3.Lerp(initialHeadPosition, targetPosition, time);
    }

    public void MoveLeftHandToTargetLocation(Vector3 targetPosition, float time)
    {
        Vector3.Lerp(initialLeftHandPosition, targetPosition, time);
    }

    public void MoveRightHandToTargetLocation(Vector3 targetPosition, float time)
    {
        Vector3.Lerp(initialRightHandPosition, targetPosition, time);
    }

    IEnumerator MoveToTargetPosition(Vector3 targetHeadPosition, Vector3 targetLeftHandPosition, Vector3 targetRightHandPosition, float time)
    {
        float timeElapsed = 0;

        Vector3 headTargetRotation = ReplayController.Instance.GetRecordingData().GetHeadTransformLogs()[0].Rotation;
        Vector3 leftHandTargetRotation = ReplayController.Instance.GetRecordingData().GetLeftHandTransformLogs()[0].Rotation;
        Vector3 rightHandTargetRotation = ReplayController.Instance.GetRecordingData().GetRightHandTransformLogs()[0].Rotation;

        Vector3 currentHeadPosition = headTransform.position;
        Vector3 currentLeftHandPosition = leftHandTransform.position;
        Vector3 currentRightHandPosition = rightHandTransform.position;

        while (timeElapsed < time)
        {
            headTransform.position = Vector3.Lerp(currentHeadPosition, targetHeadPosition, timeElapsed / time);
            //headTransform.eulerAngles = Vector3.Slerp(initialHeadRotation, headTargetRotation, timeElapsed / time);

            leftHandTransform.position = Vector3.Lerp(currentLeftHandPosition, targetLeftHandPosition, timeElapsed / time);
            //leftHandTransform.eulerAngles = Vector3.Slerp(initialLeftHandRotation, leftHandTargetRotation, timeElapsed / time);

            rightHandTransform.position = Vector3.Lerp(currentRightHandPosition, targetRightHandPosition, timeElapsed / time);
            //rightHandTransform.eulerAngles = Vector3.Slerp(initialRightHandRotation, rightHandTargetRotation, timeElapsed / time);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        FighterLoader.Instance.InvokeOnFighterInPosition();
    }

    public void MoveToStartPosition(float time)
    {
        StopAllCoroutines();

        Vector3 headTargetPosition = ReplayController.Instance.GetRecordingData().GetHeadTransformLogs()[0].Position - FighterCoordinator.Instance.offset;

        Vector3 targetPosition = new Vector3(headTargetPosition.x, 0, headTargetPosition.z);

        StartCoroutine(MoveFighterVisualsToTargetPosition(targetPosition, time));


    }

    IEnumerator MoveFighterVisualsToTargetPosition(Vector3 targetPosition, float time)
    {
        float timeElapsed = 0;

        while (timeElapsed < (time / 3) * 2)
        {
            fighterVisuals.position = Vector3.Lerp(fighterVisuals.position, targetPosition, timeElapsed / time);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        Vector3 headTargetPosition = ReplayController.Instance.GetRecordingData().GetHeadTransformLogs()[0].Position - FighterCoordinator.Instance.offset;
        Vector3 leftHandTargetPosition = ReplayController.Instance.GetRecordingData().GetLeftHandTransformLogs()[0].Position - FighterCoordinator.Instance.offset;
        Vector3 rightHandTargetPosition = ReplayController.Instance.GetRecordingData().GetRightHandTransformLogs()[0].Position - FighterCoordinator.Instance.offset;

        StartCoroutine(MoveToTargetPosition(headTargetPosition, leftHandTargetPosition, rightHandTargetPosition, time / 3));
    }

    public void ResetPositions()
    {
        fighterVisuals.localPosition = Vector3.zero;

        headTransform.position = initialHeadPosition;
        headTransform.eulerAngles = initialHeadRotation;

        leftHandTransform.position = initialLeftHandPosition;
        leftHandTransform.eulerAngles = initialLeftHandRotation;

        rightHandTransform.position = initialRightHandPosition;
        rightHandTransform.eulerAngles = initialRightHandRotation;
    }
}
