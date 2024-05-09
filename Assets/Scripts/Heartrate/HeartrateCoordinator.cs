using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
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
        if (audioSource == null) return;
        audioSource.clip = lubClip;
        audioSource.Play();
    }
    public void PlayDub()
    {
        if (audioSource == null) return;
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
    private List<MeshRenderer> meshRenderers;
    private Dictionary<MeshRenderer, Material> objMaterials;
    private List<Material> seenMaterials;
    public Color highlightColor = Color.white;
    public Color startColor = Color.black;
    public float highlightSpeed = 15f;

    public bool IsInitialized { get; private set; }

    public void Initialize(List<MeshRenderer> renderers)
    {
        seenMaterials = new List<Material>();
        meshRenderers = renderers;
        objMaterials = new Dictionary<MeshRenderer, Material>();

        foreach (MeshRenderer renderer in renderers)
        {
            objMaterials[renderer] = renderer.sharedMaterial;
        }

        IsInitialized = true;
    }

    public void Highlight()
    {
        if (!IsInitialized) { return; }
        seenMaterials.Clear();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            Material mat = objMaterials[renderer];
            if (mat != null && !seenMaterials.Contains(mat))
            {
                mat.SetColor("_EmissionColor", Color.Lerp(mat.GetColor("_EmissionColor"), highlightColor, highlightSpeed * Time.deltaTime));
                seenMaterials.Add(mat);
            }
        }
    }

    public void Dim()
    {
        if (!IsInitialized) { return; }
        seenMaterials.Clear();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            Material mat = objMaterials[renderer];
            if (mat != null && !seenMaterials.Contains(mat))
            {
                objMaterials[renderer].SetColor("_EmissionColor", Color.Lerp(objMaterials[renderer].GetColor("_EmissionColor"), startColor, highlightSpeed * Time.deltaTime));
                seenMaterials.Add(objMaterials[renderer]);
            }

        }
    }

    public void EndDim()
    {
        if (!IsInitialized) { return; }
        seenMaterials.Clear();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            Material mat = objMaterials[renderer];
            if (mat != null && !seenMaterials.Contains(mat))
            {
                objMaterials[renderer].SetColor("_EmissionColor", startColor);
                seenMaterials.Add(objMaterials[renderer]);
            }
        }
    }

    public void End()
    {
        meshRenderers = null;
        objMaterials = null;
    }
}

public class HeartrateCoordinator : NetworkBehaviour
{

    public static HeartrateCoordinator Instance { get; private set; }

    [SerializeField] private bool syncedFeedback = true;
    public event EventHandler OnVisualFeedbackChanged;
    public event EventHandler OnAudioFeedbackChanged;
    public event EventHandler OnHapticFeedbackChanged;
    public event EventHandler OnUpdateState;

    [SerializeField] private List<TMP_Text> heartrateDisplays;

    private float newBPM = 0f;
    private float lastBPM = 0f;

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

    public IHeartbeatState CurrentState { get; private set; }
    private ReplayController replayController;

    private HeartrateFeedbackControlRpcs heartrateFeedbackControlRpcs;
    private HeartrateNetworkVariables heartrateNetworkVariables;

    private Dictionary<int, HRLog> hrLogDic;

    public float CurrentLubLength { get; private set; }
    public float CurrentDubLength { get; private set; }
    public float CurrentShortPauseLength { get; private set; }
    public float CurrentLongPauseLength { get; private set; }

    public bool VisualFeedbackActivated { get; private set; }
    public bool AudioFeedbackActivated { get; private set; }
    public bool HapticFeedbackActivated { get; private set; }

    private bool FileLoaded { get; set; } = false;

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

        VisualFeedbackActivated = useVisualFeedback;
        AudioFeedbackActivated = useAudioFeedback;
        HapticFeedbackActivated = useHapticFeedback;
    }

    private void Initialize()
    {
        replayController = ReplayController.Instance;
        heartrateFeedbackControlRpcs = HeartrateFeedbackControlRpcs.Instance;
        heartrateNetworkVariables = HeartrateNetworkVariables.Instance;

        InitializeVisualFeedback();

        replayController.OnReplayControllerLoaded += OnReplayControllerLoaded;
        replayController.OnReplayControllerUnload += OnReplayControllerUnload;

        SetState(new WaitingHeartbeatState());
        audioFeedback.Initialize(gameObject);

        if (!IsServer)
        {
            heartrateNetworkVariables.audioFeedbackActivated.OnValueChanged += AudioFeedbackActivated_OnValueChanged;
            heartrateNetworkVariables.visualFeedbackActivated.OnValueChanged += VisualFeedbackActivated_OnValueChanged;
            heartrateNetworkVariables.hapticFeedbackActivated.OnValueChanged += HapticFeedbackActivated_OnValueChanged;

            AudioFeedbackActivated = heartrateNetworkVariables.audioFeedbackActivated.Value;
            VisualFeedbackActivated = heartrateNetworkVariables.visualFeedbackActivated.Value;
            HapticFeedbackActivated = heartrateNetworkVariables.hapticFeedbackActivated.Value;

        }

        OnAudioFeedbackChanged?.Invoke(this, EventArgs.Empty);
        OnVisualFeedbackChanged?.Invoke(this, EventArgs.Empty);
        OnHapticFeedbackChanged?.Invoke(this, EventArgs.Empty);
    }

    IEnumerator WaitForFighterCoordinator()
    {
        while (FighterCoordinator.Instance == null || FighterCoordinator.Instance.GetVisualMeshRenderers() == null)
        {
            yield return null;
        }

        FighterCoordinator.Instance.OnFighterInPosition += OnFighterInPosition;

    }

    private void OnFighterInPosition(object sender, EventArgs e)
    {
        visualFeedback.Initialize(FighterCoordinator.Instance.GetVisualMeshRenderers());
        FileLoaded = true;
    }

    private void InitializeVisualFeedback()
    {
        StartCoroutine(WaitForFighterCoordinator());
    }

    private void AudioFeedbackActivated_OnValueChanged(bool previousValue, bool newValue)
    {
        AudioFeedbackActivated = newValue;
        OnAudioFeedbackChanged?.Invoke(this, EventArgs.Empty);
    }

    private void VisualFeedbackActivated_OnValueChanged(bool previousValue, bool newValue)
    {
        VisualFeedbackActivated = newValue;
        OnVisualFeedbackChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HapticFeedbackActivated_OnValueChanged(bool previousValue, bool newValue)
    {
        HapticFeedbackActivated = newValue;
        OnHapticFeedbackChanged?.Invoke(this, EventArgs.Empty);
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    public void OnReplayControllerLoaded(object sender, System.EventArgs e)
    {
        hrLogDic = ReplayController.Instance.GetRecordingData().GetHRLogs();

        WaitForFighterCoordinator();
        InitializeVisualFeedback();
        SetState(new IdleHeartbeatState());
        //FileLoaded = true;
    }

    public void OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        FileLoaded = false;
        SetState(new WaitingHeartbeatState());
        visualFeedback.End();
        newBPM = lastBPM = 0;
        ChangeHeartrateDisplayTexts("0");
        hrLogDic = null;
    }

    private void ChangeHeartrateDisplayTexts(string text)
    {
        foreach (TMP_Text display in heartrateDisplays)
        {
            display.text = text;
        }
    }

    public void SetState(IHeartbeatState newState)
    {
        OnUpdateState?.Invoke(this, EventArgs.Empty);
        CurrentState = newState;
        newState.Enter(this);
    }

    public void ResetCoordinator()
    {
        if (visualFeedback != null)
        {
            visualFeedback.End();
        }
        audioFeedback.End();
        VisualFeedbackActivated = useVisualFeedback;
        AudioFeedbackActivated = useAudioFeedback;
        HapticFeedbackActivated = useHapticFeedback;
        SetState(new WaitingHeartbeatState());
        newBPM = lastBPM = 0;
        ChangeHeartrateDisplayTexts("0");
    }

    private void Update()
    {
        if (!FileLoaded) { return; }
        HandleBPM();

        if (newBPM <= 0f) { return; }
        CurrentState.Update(Instance);
    }

    private void HandleBPM()
    {
        newBPM = GetCurrentHeartRate();
        if (newBPM < 0f) { return; }
        ChangeHeartrateDisplayTexts(newBPM.ToString());

        if (newBPM == 0f) { return; }

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

    public void InitChangeAudioFeedback()
    {
        if (IsServer)
        {
            ChangeAudioFeedback();
        }
        else
        {
            heartrateFeedbackControlRpcs.ChangeAudioFeedbackServerRpc();
        }
    }

    private void ChangeAudioFeedback()
    {
        AudioFeedbackActivated = !AudioFeedbackActivated;
        OnAudioFeedbackChanged?.Invoke(this, EventArgs.Empty);
    }

    public void InitChangeVisualFeedback()
    {
        if (IsServer)
        {
            ChangeVisualFeedback();
        }
        else
        {
            heartrateFeedbackControlRpcs.ChangeVisualFeedbackServerRpc();
        }
    }

    private void ChangeVisualFeedback()
    {
        VisualFeedbackActivated = !VisualFeedbackActivated;
        visualFeedback.EndDim();
        OnVisualFeedbackChanged?.Invoke(this, EventArgs.Empty);
    }

    public void InitChangeHapticFeedback()
    {
        if (IsServer)
        {
            ChangeHapticFeedback();
        }
        else
        {
            heartrateFeedbackControlRpcs.ChangeHapticFeedbackServerRpc();
        }
    }

    private void ChangeHapticFeedback()
    {
        HapticFeedbackActivated = !HapticFeedbackActivated;
        OnHapticFeedbackChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetCurrentHeartRate()
    {
        if (!FileLoaded)
        {
            return 0;
        }

        int nearestFrame = -1;
        int activeFrame = replayController.GetActiveFrame();
        int smallestDifference = int.MaxValue;

        // Durchlaufe das Dictionary, um den nächsten Schlüssel zu finden
        foreach (var entry in hrLogDic)
        {
            int currentDifference = Math.Abs(entry.Key - activeFrame);
            if (currentDifference < smallestDifference)
            {
                smallestDifference = currentDifference;
                nearestFrame = entry.Key;
            }
        }

        // Überprüfe, ob ein nächster Frame gefunden wurde
        if (nearestFrame != -1 && hrLogDic.TryGetValue(nearestFrame, out HRLog log))
        {
            return log.heartRate;
        }

        // Wenn kein passender HRLog gefunden wurde, gebe 0 zurück
        return 0;
    }

    public bool IsFeedbackSynced()
    {
        return syncedFeedback;
    }

    private void OnDisable()
    {
        if (replayController != null)
        {
            replayController.OnReplayControllerLoaded -= OnReplayControllerLoaded;
            replayController.OnReplayControllerUnload -= OnReplayControllerUnload;
        }

        StopCoroutine(WaitForFighterCoordinator());
    }
}
