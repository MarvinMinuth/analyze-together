using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HeartrateNetworkVariables : NetworkBehaviour
{
    public static HeartrateNetworkVariables Instance;
    private HeartrateCoordinator heartrateCoordinator;
    public NetworkVariable<bool> visualFeedbackActivated = new NetworkVariable<bool>();
    public NetworkVariable<bool> audioFeedbackActivated = new NetworkVariable<bool>();
    public NetworkVariable<bool> hapticFeedbackActivated = new NetworkVariable<bool>();

    private void Awake()
    {
        Instance = this;
    }

    private void Initialize()
    {
        heartrateCoordinator = HeartrateCoordinator.Instance;

        if (IsServer)
        {
            heartrateCoordinator.OnAudioFeedbackChanged += HeartrateCoordinator_OnAudioFeedbackChanged;
            heartrateCoordinator.OnHapticFeedbackChanged += HeartrateCoordinator_OnHapticFeedbackChanged;
            heartrateCoordinator.OnVisualFeedbackChanged += HeartrateCoordinator_OnVisualFeedbackChanged;

            visualFeedbackActivated.Value = heartrateCoordinator.VisualFeedbackActivated;
            hapticFeedbackActivated.Value = heartrateCoordinator.HapticFeedbackActivated;
            audioFeedbackActivated.Value = heartrateCoordinator.AudioFeedbackActivated;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void HeartrateCoordinator_OnVisualFeedbackChanged(object sender, System.EventArgs e)
    {
        visualFeedbackActivated.Value = heartrateCoordinator.VisualFeedbackActivated;
    }

    private void HeartrateCoordinator_OnHapticFeedbackChanged(object sender, System.EventArgs e)
    {
        hapticFeedbackActivated.Value = heartrateCoordinator.HapticFeedbackActivated;
    }

    private void HeartrateCoordinator_OnAudioFeedbackChanged(object sender, System.EventArgs e)
    {
        audioFeedbackActivated.Value = heartrateCoordinator.AudioFeedbackActivated;
    }

}
