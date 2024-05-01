using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HeartControlButtons : MonoBehaviour
{
    private HeartrateCoordinator heartrateCoordinator;
    private HeartrateFeedbackControlRpcs heartrateFeedbackControlRpcs;
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

        audioFeedbackButton.onClick.AddListener(OnAudioFeedbackButtonPressed);
        visualFeedbackButton.onClick.AddListener(OnVisualFeedbackButtonPressed);
        hapticFeedbackButton.onClick.AddListener(OnHapticFeedbackButtonPressed);

        heartrateCoordinator.OnAudioFeedbackChanged += HeartrateCoordinator_OnAudioFeedbackChanged;
        heartrateCoordinator.OnHapticFeedbackChanged += HeartrateCoordinator_OnHapticFeedbackChanged;
        heartrateCoordinator.OnVisualFeedbackChanged += HeartrateCoordinator_OnVisualFeedbackChanged;

        CheckAllButtonStatus();
    }

    private void HeartrateCoordinator_OnVisualFeedbackChanged(object sender, System.EventArgs e)
    {
        CheckButtonStatus(visualFeedbackButton, heartrateCoordinator.VisualFeedbackActivated);
    }

    private void HeartrateCoordinator_OnHapticFeedbackChanged(object sender, System.EventArgs e)
    {
        CheckButtonStatus(hapticFeedbackButton, heartrateCoordinator.HapticFeedbackActivated);
    }

    private void HeartrateCoordinator_OnAudioFeedbackChanged(object sender, System.EventArgs e)
    {
        CheckButtonStatus(audioFeedbackButton, heartrateCoordinator.AudioFeedbackActivated);
    }

    public void OnAudioFeedbackButtonPressed()
    {
        heartrateCoordinator.InitChangeAudioFeedback();
    }
    public void OnVisualFeedbackButtonPressed()
    {
        heartrateCoordinator.InitChangeVisualFeedback();
    }
    public void OnHapticFeedbackButtonPressed()
    {
        heartrateCoordinator.InitChangeHapticFeedback();
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
        CheckButtonStatus(visualFeedbackButton, heartrateCoordinator.VisualFeedbackActivated);
        CheckButtonStatus(hapticFeedbackButton, heartrateCoordinator.HapticFeedbackActivated);
        CheckButtonStatus(audioFeedbackButton, heartrateCoordinator.AudioFeedbackActivated);
    }
}
