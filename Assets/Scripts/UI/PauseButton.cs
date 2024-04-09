using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : NetworkBehaviour
{
    [SerializeField] private Button button;

    private NetworkVariableSync variableSync;
    private ReplayControlRpcs controlRpcs;

    public override void OnNetworkSpawn()
    {
        variableSync = NetworkVariableSync.Instance;
        controlRpcs = ReplayControlRpcs.Instance;

        if (!variableSync.isPlaying.Value) { SetButtonActive(); }

        variableSync.isPlaying.OnValueChanged += OnIsPlayingChanged;

        button.onClick.AddListener(controlRpcs.PauseServerRpc);
    }

    private void OnIsPlayingChanged(bool previous, bool isPlaying)
    {
        if (!isPlaying)
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
