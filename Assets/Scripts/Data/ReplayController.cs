using System;
using UnityEngine;

public enum Speed
{
    Half,
    Normal,
    Double
}
public enum Direction
{
    Forwards,
    Backwards
}
public class ReplayController : MonoBehaviour
{
    public static ReplayController Instance { get; private set; }

    public event EventHandler OnReplayDataReady;
    public event EventHandler OnReplayControllerLoaded;
    public event EventHandler OnReplayControllerUnload;
    public event EventHandler<OnFrameChangedEventArgs> OnFrameChanged;
    public class OnFrameChangedEventArgs
    {
        public int frame;
    }

    public event EventHandler<OnReplayWindowSetEventArgs> OnReplayWindowSet;
    public class OnReplayWindowSetEventArgs
    {
        public int minReplayWindowFrame, maxReplayWindowFrame;
    }

    public event EventHandler OnReplayWindowReset;
    public event EventHandler OnPlay;
    public event EventHandler OnPause;
    public event EventHandler OnStop;
    public event EventHandler<OnSpeedChangedEventArgs> OnSpeedChanged;
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

    public bool loadReplayOnStart = false;

    [SerializeField] private RecordingSO loadOnStartReplaySO;
    private RecordingManager recordingManager;

    private bool isDataLoaded = false;
    private bool isPlaying;
    private bool isLooping;
    private bool isReceivingInput;
    private bool replayWindowChanged = false;

    private float playSpeed = 1;
    Direction playDirection = Direction.Forwards;

    int activeFrame;
    int totalFrames;
    int minPlayFrame = 0;
    int maxPlayFrame;
    float nextUpdate = 0f;

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
    }
    void Start()
    {
        recordingManager = RecordingManager.Instance;

        recordingManager.OnRecordingLoaded += RecordingManager_OnRecordingLoaded;

        //FighterLoader.Instance.OnFighterInPosition += FighterLoader_OnFighterInPosition;
    }

    private void FighterLoader_OnFighterInPosition(object sender, EventArgs e)
    {
        OnReplayControllerLoaded?.Invoke(this, EventArgs.Empty);
        Play();
    }

    private void RecordingManager_OnRecordingLoaded(object sender, RecordingManager.OnRecordingLoadedEventArgs e)
    {
        totalFrames = recordingManager.GetMaxFrame() + 1;
        maxPlayFrame = recordingManager.GetMaxFrame();

        OnReplayWindowSet?.Invoke(this, new OnReplayWindowSetEventArgs
        {
            minReplayWindowFrame = minPlayFrame,
            maxReplayWindowFrame = maxPlayFrame
        });

        isDataLoaded = true;

        OnReplayDataReady?.Invoke(this, EventArgs.Empty);

        SetFrame(minPlayFrame);

        //heartrateCoordinator.Setup();
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }
        // �berpr�fe, ob 1/60 Sekunde vergangen ist
        if (Time.time >= nextUpdate)
        {
            LoadNextFrame();
            // Berechne den Zeitpunkt des n�chsten Updates
            nextUpdate = Time.time + (1f / (60f * playSpeed));
        }
    }

    public void ChangeSpeed(Speed speed)
    {
        switch (speed)
        {
            case Speed.Normal: playSpeed = 1f; break;
            case Speed.Half: playSpeed = 0.5f; break;
            case Speed.Double: playSpeed = 2f; break;
        }

        OnSpeedChangedEventArgs args = new OnSpeedChangedEventArgs { speed = speed };
        OnSpeedChanged?.Invoke(this, args);
    }

    public void Play()
    {
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        isPlaying = true;
        OnPlay?.Invoke(this, EventArgs.Empty);
    }

    public void Pause()
    {
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        isPlaying = false;
        OnPause?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        // pauses replay, resets all options
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        Pause();
        ResetReplayWindow();
        SetFrame(0);
        ChangeDirection(Direction.Forwards);
        ChangeSpeed(Speed.Normal);
        ChangeLooping(false);

        OnStop?.Invoke(this, EventArgs.Empty);
    }

    public void Load(RecordingSO replaySO)
    {
        if (isDataLoaded) { Unload(); }

        recordingManager.Load(replaySO);

        OnReplayControllerLoaded?.Invoke(this, EventArgs.Empty);
    }
    public float GetPlaySpeed() { return playSpeed; }
    public Direction GetPlayDirection() { return playDirection; }

    public void SetFrame(int newFrame)
    {
        if (!isDataLoaded)
        {
            Debug.Log("No file loaded");
            return;
        }

        activeFrame = AdjustToWindow(newFrame);

        OnFrameChanged?.Invoke(this, new OnFrameChangedEventArgs
        {
            frame = activeFrame
        });
    }

    public int GetFrame()
    {
        return activeFrame;
    }

    private int AdjustToWindow(int windowFrame)
    {
        if (windowFrame < minPlayFrame) windowFrame = minPlayFrame;
        if (windowFrame > maxPlayFrame) windowFrame = maxPlayFrame;

        return windowFrame;
    }

    public void LoadNextFrame()
    {
        int nextFrame = activeFrame;
        if (playDirection == Direction.Forwards)
        {
            nextFrame += 1;
            if (nextFrame >= totalFrames)
            {
                if (isLooping)
                {
                    SetFrame(minPlayFrame);
                }
                else
                {
                    Stop();
                }
            }
            else if (nextFrame >= maxPlayFrame)
            {
                if (isLooping)
                {
                    SetFrame(minPlayFrame);
                }
                else
                {
                    Pause();
                }
            }
            else
            {
                SetFrame(nextFrame);
            }
        }
        else
        {
            nextFrame -= 1;
            if (nextFrame < 0)
            {
                if (isLooping)
                {
                    SetFrame(maxPlayFrame);
                }
                else
                {
                    Pause();
                }
            }
            else if (nextFrame < minPlayFrame)
            {
                if (isLooping)
                {
                    SetFrame(maxPlayFrame);
                }
                else { Pause(); }
            }
            else
            {
                SetFrame(nextFrame);
            }
        }
    }

    public void Unload()
    {
        Stop();
        OnReplayControllerUnload?.Invoke(this, EventArgs.Empty);

        minPlayFrame = 0;
        totalFrames = 0;
        maxPlayFrame = 0;
        recordingManager.Unload();
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public int GetReplayLength()
    {
        return recordingManager.GetMaxFrame() + 1;
    }

    public bool IsReplayReady()
    {
        return (isDataLoaded);
    }

    public void ChangeLooping()
    {
        if (isLooping) isLooping = false;
        else isLooping = true;

        OnRepeat?.Invoke(this, EventArgs.Empty);
    }
    public void ChangeLooping(bool shouldLoop)
    {
        isLooping = shouldLoop;
        OnRepeat?.Invoke(this, EventArgs.Empty);
    }

    public void ChangeDirection()
    {
        if (playDirection == Direction.Forwards) playDirection = Direction.Backwards;
        else playDirection = Direction.Forwards;

        OnDirectionChangedEventArgs args = new OnDirectionChangedEventArgs { direction = playDirection };
        OnDirectionChanged?.Invoke(this, args);
    }
    public void ChangeDirection(Direction direction)
    {
        playDirection = direction;
        OnDirectionChangedEventArgs args = new OnDirectionChangedEventArgs { direction = playDirection };
        OnDirectionChanged?.Invoke(this, args);
    }

    public bool IsLooping()
    {
        return isLooping;
    }

    public bool IsControllable()
    {
        return (isDataLoaded && !isReceivingInput);
    }

    public void SetReceivingInput(bool isReceivingInput)
    {
        this.isReceivingInput = isReceivingInput;
    }

    public void ChangeReplayWindow(int minFrame, int maxFrame)
    {
        minPlayFrame = Mathf.Min(minFrame, maxFrame);
        maxPlayFrame = Mathf.Max(minFrame, maxFrame);

        if (activeFrame < minPlayFrame) SetFrame(minPlayFrame);
        if (activeFrame > maxPlayFrame) SetFrame(maxPlayFrame);

        replayWindowChanged = true;

        OnReplayWindowSet(this, new OnReplayWindowSetEventArgs
        {
            minReplayWindowFrame = minFrame,
            maxReplayWindowFrame = maxFrame
        });
    }

    public void ResetReplayWindow()
    {
        minPlayFrame = 0;
        maxPlayFrame = totalFrames - 1;
        replayWindowChanged = false;

        OnReplayWindowReset?.Invoke(this, EventArgs.Empty);
    }

    public bool ManagerIsLoading()
    {
        return recordingManager.IsLoading();
    }

    public bool IsReplayWindowChanged()
    {
        return replayWindowChanged;
    }

    public void LoadOnStartReplay()
    {
        Debug.Log("Load on start Replay");
        Load(loadOnStartReplaySO);
    }

    public void ChangePlayPause()
    {
        isPlaying = !isPlaying;
    }

    public Savefile GetLoadedSavefile()
    {
        if (totalFrames != 0)
        {
            return Savefile.Tutorial;
        }
        else
        {
            return Savefile.None;
        }
    }
}
