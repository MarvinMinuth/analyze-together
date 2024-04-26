using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HeartrateFeedbackControlRpcs : NetworkBehaviour
{
    public static HeartrateFeedbackControlRpcs Instance;

    private HeartrateCoordinator heartrateCoordinator;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        heartrateCoordinator = HeartrateCoordinator.Instance;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeAudioFeedbackServerRpc()
    {
        heartrateCoordinator.ChangeAudioFeedback();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHapticFeedbackServerRpc()
    {
        heartrateCoordinator.ChangeHapticFeedback();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeVisualFeedbackServerRpc()
    {
        heartrateCoordinator.ChangeVisualFeedback();
    }
}
