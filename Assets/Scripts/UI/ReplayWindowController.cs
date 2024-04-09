using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReplayWindowController : NetworkBehaviour
{
    [SerializeField] private Slider maxSlider;
    [SerializeField] private Slider minSlider;
    [SerializeField] private float minDistance = 30;

    private NetworkVariableSync variableSync;
    private ReplayControlRpcs controlRpcs;
    private float length;
    private bool isLocked;
    private bool moreThanOneInteractor;
    private bool wasRunning;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        variableSync = NetworkVariableSync.Instance;
        controlRpcs = ReplayControlRpcs.Instance;

        variableSync.replayLength.OnValueChanged += OnReplayLengthChanged;
        variableSync.minFrame.OnValueChanged += OnMinFrameChanged;
        variableSync.maxFrame.OnValueChanged += OnMaxFrameChanged;
    }

    private void OnReplayLengthChanged(int previous, int current)
    {
        length = current;
        maxSlider.maxValue = current;
        maxSlider.value = current;
        minSlider.maxValue = current - minDistance;
    }

    private void OnMaxFrameChanged(int previous, int current)
    {
        maxSlider.value = current;
    }

    private void OnMinFrameChanged(int previous, int current)
    {
        minSlider.value = current;
    }

    public void ChangeReplayWindow()
    {
        ReplayControlRpcs.Instance.ChangeReplayWindowServerRpc((int)minSlider.value, (int)maxSlider.value);
    }

    public float GetMinDistance()
    {
        return minDistance;
    }
}
