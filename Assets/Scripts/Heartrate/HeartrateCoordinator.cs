using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System.Collections.Generic;
using System;
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
    private List<MeshRenderer> meshRenderers;
    private Dictionary<MeshRenderer, Material> objMaterials;
    private Dictionary<MeshRenderer, Color> objStartColors;
    public Color highlightColor = Color.white;
    public float highlightSpeed = 15f;

    public void Initialize(List<MeshRenderer> renderers)
    {
        meshRenderers = renderers;
        objMaterials = new Dictionary<MeshRenderer, Material>();
        objStartColors = new Dictionary<MeshRenderer, Color>();

        foreach (MeshRenderer renderer in renderers)
        {
            objMaterials[renderer] = renderer.material;
            objStartColors[renderer] = objMaterials[renderer].GetColor("_EmissionColor");
        }
    }

    public void Highlight()
    {
        List<Material> seenMaterials = new List<Material>();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            if (objMaterials[renderer] != null)
            {
                if (!seenMaterials.Contains(objMaterials[renderer]))
                {
                    objMaterials[renderer].SetColor("_EmissionColor", Color.Lerp(objMaterials[renderer].GetColor("_EmissionColor"), highlightColor, highlightSpeed * Time.deltaTime));
                    seenMaterials.Add(objMaterials[renderer]);
                }


            }
        }
    }

    public void Dim()
    {
        foreach (MeshRenderer renderer in meshRenderers)
        {
            List<Material> seenMaterials = new List<Material>();
            if (objMaterials[renderer] != null)
            {

                if (!seenMaterials.Contains(objMaterials[renderer]))
                {
                    objMaterials[renderer].SetColor("_EmissionColor", Color.Lerp(objMaterials[renderer].GetColor("_EmissionColor"), objStartColors[renderer], highlightSpeed * Time.deltaTime));
                    seenMaterials.Add(objMaterials[renderer]);
                }
            }
        }
    }

    public void EndDim()
    {
        foreach (MeshRenderer renderer in meshRenderers)
        {
            List<Material> seenMaterials = new List<Material>();
            if (objMaterials[renderer] != null)
            {

                if (!seenMaterials.Contains(objMaterials[renderer]))
                {
                    objMaterials[renderer].SetColor("_EmissionColor", objStartColors[renderer]);
                    seenMaterials.Add(objMaterials[renderer]);
                }
            }
        }
    }

    public void End()
    {
        meshRenderers = null;
        objMaterials = null;
        objStartColors = null;
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

    [SerializeField] private TMP_Text heartrateDisplay;

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

        if (FighterCoordinator.Instance != null && FighterCoordinator.Instance.IsInitialized)
        {
            visualFeedback.Initialize(FighterCoordinator.Instance.GetVisualMeshRenderers());
        }
        else
        {
            if (FighterCoordinator.Instance == null)
            {
                NetworkServerSetup.Instance.OnServerSetupComplete += (sender, e) =>
                {
                    visualFeedback.Initialize(FighterCoordinator.Instance.GetVisualMeshRenderers());
                };
            }
            else
            {
                FighterCoordinator.Instance.OnFighterInitialized += (sender, e) =>
                {
                    visualFeedback.Initialize(FighterCoordinator.Instance.GetVisualMeshRenderers());
                };
            }
        }


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
        visualFeedback.Initialize(FighterCoordinator.Instance.GetVisualMeshRenderers());
        SetState(new IdleHeartbeatState());
        FileLoaded = true;
    }

    public void OnReplayControllerUnload(object sender, System.EventArgs e)
    {
        FileLoaded = false;
        SetState(new WaitingHeartbeatState());
        visualFeedback.End();
        newBPM = lastBPM = 0;
        heartrateDisplay.text = "0";
        hrLogDic = null;
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
        heartrateDisplay.text = "0";
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
        heartrateDisplay.text = newBPM.ToString();

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
        foreach (int key in hrLogDic.Keys)
        {
            // Wenn der Schl�ssel kleiner oder gleich dem gegebenen Frame ist
            if (key <= replayController.GetActiveFrame())
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
        return hrLogDic[FindClosestKey(replayController.GetActiveFrame())].heartRate;
    }

    public int FindClosestKey(int targetValue)
    {
        Dictionary<int, HRLog> hrLog = replayController.GetRecordingData().GetHRLogs();
        // Verwende LINQ, um den Schlüssel zu finden, der dem gegebenen Wert am nächsten ist
        var closestKey = hrLog.Keys.OrderBy(key => Math.Abs(key - targetValue)) // Sortiere die Schlüssel nach ihrer Differenz zum Zielwert
                                   .First(); // Nimm den ersten, also den nächsten Schlüssel

        return closestKey;
    }

    public bool IsFeedbackSynced()
    {
        return syncedFeedback;
    }
}
