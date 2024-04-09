using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class BackwardsButton : NetworkBehaviour
{
    [SerializeField] private Button button;

    private NetworkVariableSync variableSync;
    private ReplayControlRpcs controlRpcs;

    public override void OnNetworkSpawn()
    {
        variableSync = NetworkVariableSync.Instance;
        controlRpcs = ReplayControlRpcs.Instance;

        if(variableSync.direction.Value == Direction.Backwards) { SetButtonActive(); }

        variableSync.direction.OnValueChanged += OnReplayDirectionChanged;

        button.onClick.AddListener(controlRpcs.ChangeDirectionServerRpc);
    }

    private void OnReplayDirectionChanged(Direction previous, Direction current)
    {
        if (current == Direction.Backwards)
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
