using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections.Generic;
using System;
using Unity.Netcode;

[System.Serializable]
public class HapticFeedback
{
    [SerializeField] private XRBaseController leftXRController, rightXRController;
    [Range(0f, 1f)]
    public float intensity = 0.5f;
    [Range(0.01f, 0.2f)]
    public float duration = 0.1f;

    public void TriggerHaptic()
    {
        if (intensity > 0 && leftXRController != null && rightXRController != null)
        {
            leftXRController.SendHapticImpulse(intensity, duration);
            rightXRController.SendHapticImpulse(intensity, duration);
        }
    }
}

[System.Serializable]
public class AudioFeedback
{
    private AudioSource audioSource;
    public AudioClip lubClip, dubClip;
    public void Initialize(GameObject obj)
    {
        audioSource = obj.GetComponent<AudioSource>();
        audioSource.loop = false;
    }
    public void PlayLub()
    {
        audioSource.clip = lubClip;
        audioSource.Play();
    }
    public void PlayDub()
    {
        audioSource.clip = dubClip;
        audioSource.Play();
    }
    public void End()
    {
        audioSource.clip = null;
    }
}

[System.Serializable]
public class VisualFeedback
{
    private Vector3 startScale;
    private Vector3 endScale;
    private GameObject obj;

    public float scaleFactor = 1.5f;
    public float scaleSpeed = 25f;

    public void Initialize(GameObject obj)
    {
        this.obj = obj;
        startScale = obj.transform.localScale;
        endScale = new Vector3(startScale.x * scaleFactor, startScale.y * scaleFactor, startScale.z * scaleFactor);
    }

    public void ScaleUp()
    {
        obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, endScale, scaleSpeed * Time.deltaTime);
    }

    public void ScaleDown()
    {
        obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, startScale, scaleSpeed * Time.deltaTime);
    }

    public void End()
    {
        obj.transform.localScale = startScale;
    }
}

public class HeartrateCoordinator : NetworkBehaviour
{

    public static HeartrateCoordinator Instance { get; private set; }

    [SerializeField] private bool syncedFeedback = true;
    public event EventHandler OnVisualFeedbackChanged;
    public event EventHandler OnAudioFeedbackChanged;
    public event EventHandler OnHapticFeedbackChanged;

    [SerializeField] private TMP_Text heartrateDisplay;

    private float newBPM = 80f;
    private float lastBPM = 80f;

    [Header("Feedback Options")]
    public bool useVisualFeedback = true;
    public bool useAudioFeedback = false;
    public bool useHapticFeedback = false;

    [Header("Interval Length")]
    [SerializeField] private float shortPause = 2f;
    [SerializeField] private float longPause = 4f;
    [SerializeField] private float lubPhase = 1f;
    [SerializeField] private float dubPhase = 1f;

    private float beatInterval;

    public AudioFeedback audioFeedback;
    public HapticFeedback hapticFeedback;
    public VisualFeedback visualFeedback;

    private IHeartbeatState currentState;
    private ReplayController replayController;

    private Dictionary<int, HRLog> hrLogDic;

    public float CurrentLubLength { get; private set; }
    public float CurrentDubLength { get; private set; }
    public float CurrentShortPauseLength { get; private set; }
    public float CurrentLongPauseLength { get; private set; }

    public bool visualFeedbackActivated;
    public bool audioFeedbackActivated;
    public bool hapticFeedbackActivated;

    private bool fileLoaded = false;

    private NetworkVariableSync networkVariableSync;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one HeartrateCoordinator found");
        }

        //visualFeedbackActivated = useVisualFeedback;
        audioFeedbackActivated = useAudioFeedback;
        hapticFeedbackActivated = useHapticFeedback;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        networkVariableSync = NetworkVariableSync.Instance;

        networkVariableSync.isRecordingLoaded.OnValueChanged += OnRecordingLoadedChanged;

        SetState(new WaitingHeartbeatState());

        audioFeedback.Initialize(gameObject);

    }

    private void OnRecordingLoadedChanged(bool previous, bool current)
    {
        if (!current) // Unloaded
        {
            fileLoaded = false;
            //visualFeedback.End();
            //audioFeedback.End();
            SetState(new WaitingHeartbeatState());
            newBPM = lastBPM = 80;
            hrLogDic = null;
        }
        else // Loaded
        {
            hrLogDic = RecordingManager.Instance.GetHRLogs();
            SetState(new IdleHeartbeatState());
            fileLoaded = true;
        }
    }

    public void SetState(IHeartbeatState newState)
    {
        currentState = newState;
        newState.Enter(this);
    }

    public void ResetCoordinator()
    {
        //visualFeedback.End();
        //audioFeedback.End();
        visualFeedbackActivated = useVisualFeedback;
        audioFeedbackActivated = useAudioFeedback;
        hapticFeedbackActivated = useHapticFeedback;
        SetState(new WaitingHeartbeatState());
        newBPM = lastBPM = 80;
    }

    private void Update()
    {
        if (!fileLoaded) { return; }
        HandleBPM();
        currentState.Update(Instance);
    }

    private void HandleBPM()
    {
        newBPM = GetCurrentHeartRate();
        if (newBPM < 0f) { return; }
        heartrateDisplay.text = newBPM.ToString();
        if (newBPM != lastBPM)
        {
            UpdatePhaseLengths();
            lastBPM = newBPM;
        }
    }

    private void UpdatePhaseLengths()
    {
        float intervalLength = lubPhase + dubPhase + shortPause + longPause;
        beatInterval = 60.0f / newBPM / intervalLength;

        CurrentLubLength = beatInterval * lubPhase;
        CurrentDubLength = beatInterval * dubPhase;
        CurrentShortPauseLength = beatInterval * shortPause;
        CurrentLongPauseLength = beatInterval * longPause;
    }

    public bool ChangeAudioFeedback()
    {
        audioFeedbackActivated = !audioFeedbackActivated;

        OnAudioFeedbackChanged?.Invoke(this, EventArgs.Empty);
        return audioFeedbackActivated;
    }

    public bool ChangeVisualFeedback()
    {
        visualFeedbackActivated = !visualFeedbackActivated;

        OnVisualFeedbackChanged?.Invoke(this, EventArgs.Empty);
        return visualFeedbackActivated;
    }

    public bool ChangeHapticFeedback()
    {
        hapticFeedbackActivated = !hapticFeedbackActivated;

        OnHapticFeedbackChanged?.Invoke(this, EventArgs.Empty);
        return hapticFeedbackActivated;
    }

    public bool IsAudioFeedbackActivated()
    {
        return audioFeedbackActivated;
    }
    public bool IsVisualFeedbackActivated()
    {
        return visualFeedbackActivated;
    }
    public bool IsHapticFeedbackActivated()
    {
        return hapticFeedbackActivated;
    }
    public int GetCurrentHeartRate()
    {
        if (!fileLoaded)
        {
            return 0;
        }
        int nearestFrame = -1;
        foreach (int key in hrLogDic.Keys)
        {
            // Wenn der Schl�ssel kleiner oder gleich dem gegebenen Frame ist
            if (key <= networkVariableSync.activeFrame.Value)
            {
                // Aktualisiere den Wert von nearestFrame, wenn der Schl�ssel gr��er ist
                if (key > nearestFrame)
                {
                    nearestFrame = key;
                }
            }
        }

        // Wenn nearestFrame aktualisiert wurde, gib den entsprechenden HRLog zur�ck
        if (nearestFrame != -1)
        {
            return hrLogDic[nearestFrame].heartRate;
        }

        // Wenn kein passender HRLog gefunden wurde, gib null zur�ck
        return 0;
    }

    public bool IsFeedbackSynced()
    {
        return syncedFeedback;
    }
}
