using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class StopButton : NetworkBehaviour
{
    [SerializeField] private Button button;

    private ReplayControlRpcs controlRpcs;

    public override void OnNetworkSpawn()
    {
        controlRpcs = ReplayControlRpcs.Instance;

        button.onClick.AddListener(controlRpcs.StopServerRpc);
    }
}
