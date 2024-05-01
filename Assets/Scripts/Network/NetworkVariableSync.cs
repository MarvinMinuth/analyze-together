using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkVariableSync : NetworkBehaviour
{
    public static NetworkVariableSync Instance;

    private ReplayController replayController;
    private HeartrateCoordinator heartrateCoordinator;

    public NetworkVariable<bool> isInteractionInProgress = new NetworkVariable<bool>();
    public NetworkVariable<ulong> interactorId = new NetworkVariable<ulong>();
    public NetworkVariable<bool> isRecordingLoaded = new NetworkVariable<bool>();

    public NetworkVariable<bool> visualFeedbackEnabled = new NetworkVariable<bool>();
    public NetworkVariable<bool> audioFeedbackEnabled = new NetworkVariable<bool>();
    public NetworkVariable<bool> hapticFeedbackEnabled = new NetworkVariable<bool>();

    private void Awake()
    {
        Instance = this;
        isRecordingLoaded.Value = false;
    }

    private void Start()
    {

    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {

            /*
            heartrateCoordinator = HeartrateCoordinator.Instance;
            visualFeedbackEnabled.Value = heartrateCoordinator.visualFeedbackActivated;
            audioFeedbackEnabled.Value = heartrateCoordinator.audioFeedbackActivated;
            hapticFeedbackEnabled.Value = heartrateCoordinator.hapticFeedbackActivated;

            heartrateCoordinator.OnVisualFeedbackChanged += HeartrateCoordinator_OnVisualFeedbackChanged;
            heartrateCoordinator.OnAudioFeedbackChanged += HeartrateCoordinator_OnAudioFeedbackChanged;
            heartrateCoordinator.OnHapticFeedbackChanged += HeartrateCoordinator_OnHapticFeedbackChanged;
            */
        }
    }

    private void HeartrateCoordinator_OnHapticFeedbackChanged(object sender, System.EventArgs e)
    {
        hapticFeedbackEnabled.Value = heartrateCoordinator.HapticFeedbackActivated;
    }

    private void HeartrateCoordinator_OnAudioFeedbackChanged(object sender, System.EventArgs e)
    {
        audioFeedbackEnabled.Value = heartrateCoordinator.AudioFeedbackActivated;
    }

    private void HeartrateCoordinator_OnVisualFeedbackChanged(object sender, System.EventArgs e)
    {
        visualFeedbackEnabled.Value = heartrateCoordinator.VisualFeedbackActivated;
    }


}
