using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LoopButton : NetworkBehaviour
{
    [SerializeField] private Button button;

    private NetworkVariableSync variableSync;
    private ReplayControlRpcs controlRpcs;

    public override void OnNetworkSpawn()
    {
        variableSync = NetworkVariableSync.Instance;
        controlRpcs = ReplayControlRpcs.Instance;

        if (variableSync.isLooping.Value) { SetButtonActive(); }

        variableSync.isLooping.OnValueChanged += OnIsLoopingChanged;

        button.onClick.AddListener(controlRpcs.RepeatServerRpc);
    }

    private void OnIsLoopingChanged(bool previous, bool isLooping)
    {
        if (isLooping)
        {
            SetButtonActive();
        }
        else
        {
            SetButtonInactive();
        }
    }

    public void SetButtonActive()
    {
        button.GetComponent<Image>().color = Color.grey;
    }

    public void SetButtonInactive()
    {
        button.GetComponent<Image>().color = Color.white;
    }
}
