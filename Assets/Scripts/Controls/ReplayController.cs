using System;
using Unity.Netcode;
using UnityEngine;

public enum Speed
{
    Half,
    Normal,
    Double
}
public enum Direction
{
    Forward,
    Backward
}
public class ReplayController : NetworkBehaviour
{
    public static ReplayController Instance { get; private set; }
    public class OnStartLoadingEventArgs : EventArgs
    {
        public SaveFile saveFile;
    }

    public event EventHandler OnReplayDataReady;
    public event EventHandler OnReplayControllerLoaded;
    public event EventHandler OnReplayControllerUnload;
    public event EventHandler<OnActiveFrameChangedEventArgs> OnActiveFrameChanged;
    public class OnActiveFrameChangedEventArgs
    {
        public int newActiveFrame;
    }

    public event EventHandler<OnReplayWindowSetEventArgs> OnReplayWindowSet;
    public class OnReplayWindowSetEventArgs
    {
        public int minReplayWindowFrame, maxReplayWindowFrame;
    }

    public event EventHandler OnReplayWindowReset;
    public event EventHandler OnPlay;
    public event EventHandler OnPause;
    public class OnSpeedChangedEventArgs : EventArgs
    {
        public Speed speed;
    }

    public event EventHandler<OnDirectionChangedEventArgs> OnDirectionChanged;
    public class OnDirectionChangedEventArgs : EventArgs
    {
        public Direction direction;
    }
    public event EventHandler OnRepeat;

    [SerializeField] private RecordingSO loadOnStartReplaySO;
    private RecordingManager recordingManager;

    private bool isDataLoaded = false;
    private bool isPlaying;
    private bool isLooping;

    private float playSpeed = 1;
    Direction playDirection = Direction.Forward;

    int activeFrame = 0;
    int minPlayFrame = 0;
    int maxPlayFrame = 1;
    float nextUpdate = 0f;
    private RecordingData recordingData;
    private ReplayControllerNetworkVariables replayControllerNetworkVariables;

    private ReplayControlRpcs replayControlRpcs;

    public bool IsInitialized { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than one ReplayController found");
        }

        IsInitialized = false;
    }

    private void Initialize()
    {
        recordingManager = RecordingManager.Instance;
        replayControllerNetworkVariables = ReplayControllerNetworkVariables.Instance;
        replayControlRpcs = ReplayControlRpcs.Instance;

        recordingManager.OnRecordingLoaded += RecordingManager_OnRecordingLoaded;

        if (!IsServer)
        {
            Debug.Log("Client");
            replayControllerNetworkVariables.activeFrame.OnValueChanged += Server_OnActiveFrameChanged;
            replayControllerNetworkVariables.isPlaying.OnValueChanged += Server_OnIsPlayingChanged;
            replayControllerNetworkVariables.direction.OnValueChanged += Server_OnDirectionChanged;
            replayControllerNetworkVariables.isLooping.OnValueChanged += Server_OnRepeatChanged;
            replayControllerNetworkVariables.minPlayFrame.OnValueChanged += Server_OnMinPlayFrameChanged;
            replayControllerNetworkVariables.maxPlayFrame.OnValueChanged += Server_OnMaxPlayFrameChanged;
            replayControllerNetworkVariables.saveFile.OnValueChanged += Server_OnSaveFileChanged;

            if (replayControllerNetworkVariables.saveFile.Value != SaveFile.None)
            {
                Load(replayControllerNetworkVariables.saveFile.Value);
                activeFrame = replayControllerNetworkVariables.activeFrame.Value;
                maxPlayFrame = replayControllerNetworkVariables.maxPlayFrame.Value;
                minPlayFrame = replayControllerNetworkVariables.minPlayFrame.Value;
                isPlaying = replayControllerNetworkVariables.isPlaying.Value;
                isLooping = replayControllerNetworkVariables.isLooping.Value;
                playDirection = replayControllerNetworkVariables.direction.Value;


                if (isPlaying) OnPlay?.Invoke(this, EventArgs.Empty);
                else OnPause?.Invoke(this, EventArgs.Empty);

                OnReplayWindowSet?.Invoke(this, new OnReplayWindowSetEventArgs
                {
                    minReplayWindowFrame = minPlayFrame,
                    maxReplayWindowFrame = maxPlayFrame
                });
            }

            OnDirectionChanged?.Invoke(this, new OnDirectionChangedEventArgs { direction = replayControllerNetworkVariables.direction.Value });
            OnRepeat?.Invoke(this, EventArgs.Empty);
        }

        IsInitialized = true;

        //FighterLoader.Instance.OnFighterInPosition += FighterLoader_OnFighterInPosition;
    }

    private void Server_OnSaveFileChanged(SaveFile previousValue, SaveFile newValue)
    {
        if (newValue == SaveFile.None)
        {
            isDataLoaded = false;
            Unload();
        }
        else
        {
            Load(newValue);
        }
    }
    private void Server_OnMinPlayFrameChanged(int previousValue, int newValue)
    {
        if (!isDataLoaded) { return; }
        ChangeReplayWindow(newValue, maxPlayFrame);
    }

    private void Server_OnMaxPlayFrameChanged(int previousValue, int newValue)
    {
        if (!isDataLoaded) { return; }
        ChangeReplayWindow(minPlayFrame, newValue);
    }
    private void Server_OnRepeatChanged(bool previousValue, bool newValue)
    {
        ChangeLooping(newValue);
    }

    private void Server_OnDirectionChanged(Direction previousValue, Direction newValue)
    {
        ChangeDirection(newValue);
    }

    private void Server_OnIsPlayingChanged(bool previousValue, bool newValue)
    {
        if (!isDataLoaded) { return; }
        if (newValue)
        {
            Play();
        }
        else
        {
            Pause();
        }
    }

    private void Server_OnActiveFrameChanged(int previousValue, int newValue)
    {
        if (!isDataLoaded) { return; }
        SetFrame(newValue);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void FighterLoader_OnFighterInPosition(object sender, EventArgs e)
    {
        OnReplayControllerLoaded?.Invoke(this, EventArgs.Empty);
        Play();
    }

    private void RecordingManager_OnRecordingLoaded(object sender, RecordingManager.OnRecordingLoadedEventArgs e)
    {
        recordingData = e.loadedRecordingData;

        minPlayFrame = 0;
        maxPlayFrame = recordingData.GetMaxFrame();
        activeFrame = 0;

        isDataLoaded = true;

        OnReplayDataReady?.Invoke(this, EventArgs.Empty);
    }

    private void Update()
    {
        if (!isPlaying || !IsServer)
        {
            return;
        }
        // Ueberpruefe, ob 1/60 Sekunde vergangen ist
        if (Time.time >= nextUpdate)
        {
            SetFrame(ChooseNextFrame());
            // Berechne den Zeitpunkt des naechsten Updates
            nextUpdate = Time.time + (1f / (60f * playSpeed));
        }
    }

    public void InitPlay()
    {
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        if (IsServer)
        {
            Play();
        }
        else
        {
            replayControlRpcs.PlayServerRpc();
        }
    }

    private void Play()
    {
        isPlaying = true;
        OnPlay?.Invoke(this, EventArgs.Empty);
    }

    public void InitPause()
    {
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        if (IsServer)
        {
            Pause();
        }
        else
        {
            replayControlRpcs.PauseServerRpc();
        }
    }
    private void Pause()
    {
        isPlaying = false;
        OnPause?.Invoke(this, EventArgs.Empty);
    }

    public void InitStop()
    {
        // pauses replay, resets all options
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        if (IsServer)
        {
            Stop();
        }
        else
        {
            replayControlRpcs.StopServerRpc();
        }
    }

    private void Stop()
    {
        Pause();
        ResetReplayWindow();
        SetFrame(0);
        ChangeDirection(Direction.Forward);
        ChangeLooping(false);
    }

    public void InitLoad(SaveFile saveFile)
    {
        if (IsServer)
        {
            Load(saveFile);
        }
        else
        {
            // tell Server to load
        }
    }

    private void Load(SaveFile saveFile)
    {
        if (isDataLoaded) { Unload(); }
        recordingManager.Load(saveFile);
        OnReplayControllerLoaded?.Invoke(this, EventArgs.Empty);
        IsInitialized = true;

        if (IsServer)
        {
            ChangeReplayWindow(0, recordingData.GetMaxFrame());
            SetFrame(0);
        }
        else
        {
            ChangeReplayWindow(replayControllerNetworkVariables.minPlayFrame.Value, replayControllerNetworkVariables.maxPlayFrame.Value);
            SetFrame(replayControllerNetworkVariables.activeFrame.Value);
        }
    }

    public void InitSetFrame(int newFrame)
    {
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        if (newFrame == -1)
        {
            InitPause();
            return;
        }

        if (IsServer)
        {
            SetFrame(newFrame);
        }
        else
        {
            replayControlRpcs.SetFrameServerRpc(newFrame);
        }

    }

    private void SetFrame(int newFrame)
    {
        if (newFrame < 0)
        {
            Pause();
            return;
        }
        activeFrame = newFrame;
        OnActiveFrameChanged?.Invoke(this, new OnActiveFrameChangedEventArgs
        {
            newActiveFrame = activeFrame
        });
    }

    private int ChooseNextFrame()
    {
        int nextFrame = activeFrame;
        if (playDirection == Direction.Forward)
        {
            nextFrame += 1;
            if (nextFrame > maxPlayFrame)
            {
                if (isLooping)
                {
                    nextFrame = minPlayFrame;
                }
                else
                {
                    nextFrame = -1;
                }
            }
        }
        else
        {
            nextFrame -= 1;
            if (nextFrame < minPlayFrame)
            {
                if (isLooping)
                {
                    nextFrame = maxPlayFrame;
                }
                else
                {
                    nextFrame = -1;
                }
            }
        }
        return nextFrame;
    }

    public void InitUnload()
    {
        if (IsServer)
        {
            Unload();
        }
        else
        {
            // tell Server to unload
        }
    }

    private void Unload()
    {
        //Stop();
        OnReplayControllerUnload?.Invoke(this, EventArgs.Empty);

        isPlaying = false;
        minPlayFrame = 0;
        maxPlayFrame = 1;
        activeFrame = 0;

        recordingData = null;
        recordingManager.Unload();
        isDataLoaded = false;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public void InitChangeLooping()
    {
        if (IsServer)
        {
            ChangeLooping();
        }
        else
        {
            replayControlRpcs.RepeatServerRpc();
        }
    }

    private void ChangeLooping()
    {
        if (isLooping) isLooping = false;
        else isLooping = true;

        OnRepeat?.Invoke(this, EventArgs.Empty);
    }

    public void InitChangeLooping(bool shouldLoop)
    {
        if (IsServer)
        {
            ChangeLooping(shouldLoop);
        }
        else
        {
            replayControlRpcs.RepeatServerRpc(shouldLoop);
        }
    }

    private void ChangeLooping(bool shouldLoop)
    {
        isLooping = shouldLoop;
        OnRepeat?.Invoke(this, EventArgs.Empty);
    }

    public void InitChangeDirection()
    {
        if (IsServer)
        {
            ChangeDirection();
        }
        else
        {
            replayControlRpcs.ChangeDirectionServerRpc();
        }
    }

    private void ChangeDirection()
    {
        if (playDirection == Direction.Forward) playDirection = Direction.Backward;
        else playDirection = Direction.Forward;

        OnDirectionChangedEventArgs args = new OnDirectionChangedEventArgs { direction = playDirection };
        OnDirectionChanged?.Invoke(this, args);
    }

    public void InitChangeDirection(Direction direction)
    {
        if (IsServer)
        {
            ChangeDirection(direction);
        }
        else
        {
            replayControlRpcs.ChangeDirectionServerRpc(direction);
        }
    }

    private void ChangeDirection(Direction direction)
    {
        playDirection = direction;
        OnDirectionChangedEventArgs args = new OnDirectionChangedEventArgs { direction = playDirection };
        OnDirectionChanged?.Invoke(this, args);
    }

    public bool IsLooping()
    {
        return isLooping;
    }

    public void InitChangeReplayWindow(int minFrame, int maxFrame)
    {
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        if (IsServer)
        {
            ChangeReplayWindow(minFrame, maxFrame);
        }
        else
        {
            replayControlRpcs.ChangeReplayWindowServerRpc(minFrame, maxFrame);
        }
    }

    private void ChangeReplayWindow(int minFrame, int maxFrame)
    {
        minPlayFrame = Mathf.Min(minFrame, maxFrame);
        maxPlayFrame = Mathf.Max(minFrame, maxFrame);

        if (activeFrame < minPlayFrame) SetFrame(minPlayFrame);
        if (activeFrame > maxPlayFrame) SetFrame(maxPlayFrame);

        OnReplayWindowSet?.Invoke(this, new OnReplayWindowSetEventArgs
        {
            minReplayWindowFrame = minFrame,
            maxReplayWindowFrame = maxFrame
        });
    }

    public void InitResetReplayWindow()
    {
        if (IsServer)
        {
            ResetReplayWindow();
        }
        else
        {
            replayControlRpcs.ChangeReplayWindowServerRpc(0, recordingData.GetMaxFrame());
        }
    }

    private void ResetReplayWindow()
    {
        minPlayFrame = 0;
        maxPlayFrame = recordingData.GetMaxFrame();

        OnReplayWindowReset?.Invoke(this, EventArgs.Empty);
    }

    public SaveFile GetLoadedSaveFile()
    {
        return recordingData.GetSaveFile();
    }

    public int GetMinPlayFrame()
    {
        return minPlayFrame;
    }

    public int GetMaxPlayFrame()
    {
        return maxPlayFrame;
    }

    public int GetMaxFrame()
    {
        return recordingData.GetMaxFrame();
    }

    public Direction GetDirection()
    {
        return playDirection;
    }

    public int GetActiveFrame()
    {
        return activeFrame;
    }

    public RecordingData GetRecordingData()
    {
        return recordingData;
    }
}
