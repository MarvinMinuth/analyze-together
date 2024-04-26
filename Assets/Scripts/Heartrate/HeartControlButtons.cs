using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HeartControlButtons : NetworkBehaviour
{
    private HeartrateCoordinator heartrateCoordinator;
    private HeartrateFeedbackControlRpcs heartrateFeedbackControlRpcs;
    private NetworkVariableSync networkVariableSync;
    [SerializeField] private Button audioFeedbackButton;
    [SerializeField] private Button visualFeedbackButton;
    [SerializeField] private Button hapticFeedbackButton;

    private void OnEnable()
    {
        if (heartrateCoordinator == null)
        {
            heartrateCoordinator = HeartrateCoordinator.Instance;
        }
        if (heartrateFeedbackControlRpcs == null)
        {
            heartrateFeedbackControlRpcs = HeartrateFeedbackControlRpcs.Instance;
        }
    }
    void Start()
    {
        if (heartrateCoordinator == null)
        {
            heartrateCoordinator = HeartrateCoordinator.Instance;
        }
        if (heartrateFeedbackControlRpcs == null)
        {
            heartrateFeedbackControlRpcs = HeartrateFeedbackControlRpcs.Instance;
        }

        if (!heartrateCoordinator.IsFeedbackSynced())
        {
            audioFeedbackButton.onClick.AddListener(OnAudioFeedbackButtonPressed);
            //visualFeedbackButton.onClick.AddListener(OnVisualFeedbackButtonPressed);
            hapticFeedbackButton.onClick.AddListener(OnHapticFeedbackButtonPressed);

            heartrateCoordinator.OnAudioFeedbackChanged += HeartrateCoordinator_OnAudioFeedbackChanged;
            heartrateCoordinator.OnHapticFeedbackChanged += HeartrateCoordinator_OnHapticFeedbackChanged;
            heartrateCoordinator.OnVisualFeedbackChanged += HeartrateCoordinator_OnVisualFeedbackChanged;

            CheckAllButtonStatus();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        networkVariableSync = NetworkVariableSync.Instance;
        if (heartrateCoordinator.IsFeedbackSynced())
        {
            audioFeedbackButton.onClick.AddListener(heartrateFeedbackControlRpcs.ChangeAudioFeedbackServerRpc);
            visualFeedbackButton.onClick.AddListener(heartrateFeedbackControlRpcs.ChangeVisualFeedbackServerRpc);
            hapticFeedbackButton.onClick.AddListener(heartrateFeedbackControlRpcs.ChangeHapticFeedbackServerRpc);

            networkVariableSync.audioFeedbackEnabled.OnValueChanged += NetworkVariableSyn_OnAudioFeedbackChanged;
            networkVariableSync.hapticFeedbackEnabled.OnValueChanged += NetworkVariableSyn_OnHapticFeedbackChanged;
            networkVariableSync.visualFeedbackEnabled.OnValueChanged += NetworkVariableSyn_OnVisualFeedbackChanged;

            CheckAllButtonStatus();
        }
    }

    private void NetworkVariableSyn_OnVisualFeedbackChanged(bool previous, bool current)
    {
        CheckButtonStatus(visualFeedbackButton, current);
    }

    private void NetworkVariableSyn_OnHapticFeedbackChanged(bool previous, bool current)
    {
        CheckButtonStatus(hapticFeedbackButton, current);
    }

    private void NetworkVariableSyn_OnAudioFeedbackChanged(bool previous, bool current)
    {
        CheckButtonStatus(audioFeedbackButton, current);
    }

    private void HeartrateCoordinator_OnVisualFeedbackChanged(object sender, System.EventArgs e)
    {
        CheckButtonStatus(visualFeedbackButton, heartrateCoordinator.IsVisualFeedbackActivated());
    }

    private void HeartrateCoordinator_OnHapticFeedbackChanged(object sender, System.EventArgs e)
    {
        CheckButtonStatus(hapticFeedbackButton, heartrateCoordinator.IsHapticFeedbackActivated());
    }

    private void HeartrateCoordinator_OnAudioFeedbackChanged(object sender, System.EventArgs e)
    {
        CheckButtonStatus(audioFeedbackButton, heartrateCoordinator.IsAudioFeedbackActivated());
    }

    public void OnAudioFeedbackButtonPressed()
    {
        heartrateCoordinator.ChangeAudioFeedback();
    }
    public void OnVisualFeedbackButtonPressed()
    {
        heartrateCoordinator.ChangeVisualFeedback();
    }
    public void OnHapticFeedbackButtonPressed()
    {
        heartrateCoordinator.ChangeHapticFeedback();
    }
    public void SetButtonActive(Button button)
    {
        button.GetComponent<Image>().color = Color.grey;
    }
    public void SetButtonInactive(Button button)
    {
        button.GetComponent<Image>().color = Color.white;
    }
    public void CheckButtonStatus(Button button, bool active)
    {
        if (active)
        {
            SetButtonActive(button);
        }
        else
        {
            SetButtonInactive(button);
        }
    }

    public void CheckAllButtonStatus()
    {
        if (heartrateCoordinator.IsFeedbackSynced())
        {
            CheckButtonStatus(visualFeedbackButton, networkVariableSync.visualFeedbackEnabled.Value);
            CheckButtonStatus(hapticFeedbackButton, networkVariableSync.hapticFeedbackEnabled.Value);
            CheckButtonStatus(audioFeedbackButton, networkVariableSync.audioFeedbackEnabled.Value);
        }
        else
        {
            CheckButtonStatus(visualFeedbackButton, heartrateCoordinator.IsVisualFeedbackActivated());
            CheckButtonStatus(hapticFeedbackButton, heartrateCoordinator.IsHapticFeedbackActivated());
            CheckButtonStatus(audioFeedbackButton, heartrateCoordinator.IsAudioFeedbackActivated());
        }
    }
}
