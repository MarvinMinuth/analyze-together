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

    [SerializeField] private ReplaySO loadOnStartReplaySO;
    private ReplayManager replayManager;

    private bool isRunning;
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
        if(Instance == null)
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
        replayManager = ReplayManager.Instance;

        replayManager.OnReplayLoaded += ReplayManager_OnReplayLoaded;

        //FighterLoader.Instance.OnFighterInPosition += FighterLoader_OnFighterInPosition;
    }

    private void FighterLoader_OnFighterInPosition(object sender, EventArgs e)
    {
        OnReplayControllerLoaded?.Invoke(this, EventArgs.Empty);
        Play();
    }

    private void ReplayManager_OnReplayLoaded(object sender, ReplayManager.OnReplayLoadedEventArgs e)
    {
        totalFrames = replayManager.GetReplayLength();
        Pause();
        ResetReplayWindow();

        OnReplayDataReady?.Invoke(this, EventArgs.Empty);

        SetFrame(minPlayFrame);

        //heartrateCoordinator.Setup();
    }

    private void Update()
    {
        if (loadReplayOnStart) 
        {
            //FighterLoader.Instance.LoadReplay(loadOnStartLoadingStatueSO.saveFile);
            replayManager.Load(loadOnStartReplaySO);
            loadReplayOnStart = false;
        }

        if (!isRunning)
        {
            return;
        }
        // Überprüfe, ob 1/60 Sekunde vergangen ist
        if (Time.time >= nextUpdate)
        {
            LoadNextFrame();
            // Berechne den Zeitpunkt des nächsten Updates
            nextUpdate = Time.time + (1f / (60f * playSpeed));
        }
    }

    public void ChangeSpeed(Speed speed)
    {
        switch (speed)
        {
            case Speed.Normal:  playSpeed = 1f; break;
            case Speed.Half: playSpeed = 0.5f; break;
            case Speed.Double: playSpeed = 2f; break;
        }

        OnSpeedChangedEventArgs args = new OnSpeedChangedEventArgs { speed = speed };
        OnSpeedChanged?.Invoke(this, args);
    }
    public void Play()
    {
        if (!replayManager.FileIsLoaded())
        {
            Debug.Log("No file loaded");
            return;
        }

        isRunning = true;
        OnPlay?.Invoke(this, EventArgs.Empty);
    }
    public void Pause()
    {
        isRunning = false;
        OnPause?.Invoke(this, EventArgs.Empty);
    }
    public void Stop()
    {
        Pause();
        ResetReplayWindow();
        SetFrame(0);
        ChangeDirection(Direction.Forwards);
        ChangeSpeed(Speed.Normal);
        ChangeLooping(false);

        OnStop?.Invoke(this, EventArgs.Empty);
    }
    public void Load(ReplaySO replaySO)
    {
        if (replayManager.FileIsLoaded()) { Unload(); }

        replayManager.Load(replaySO);

        OnReplayControllerLoaded?.Invoke(this, EventArgs.Empty);
    }
    public float GetPlaySpeed() { return playSpeed; }
    public Direction GetPlayDirection() { return playDirection; }

    public void SetFrame(int newFrame)
    {
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
            else if (activeFrame >= maxPlayFrame)
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
        //heartrateCoordinator.ResetCoordinator();
        replayManager.Unload();  
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public int GetReplayLength()
    {
        return replayManager.GetReplayLength();
    }

    public bool IsReplayReady()
    {
        return (!replayManager.IsLoading() && replayManager.FileIsLoaded());
    }

    public void ChangeLooping()
    {
        if(isLooping) isLooping = false;
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
        return (IsReplayReady() && !isReceivingInput);
    }

    public void SetReceivingInput(bool isReceivingInput)
    {
        this.isReceivingInput = isReceivingInput;
    }

    public void ChangeReplayWindow(int minFrame, int maxFrame)
    {
        minPlayFrame = Mathf.Min(minFrame, maxFrame);
        maxPlayFrame = Mathf.Max(minFrame, maxFrame);
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
        maxPlayFrame = totalFrames;
        replayWindowChanged = false;

        OnReplayWindowReset?.Invoke(this, EventArgs.Empty);
    }

    public bool ManagerIsLoading()
    {
        return replayManager.IsLoading();
    }

    public bool IsReplayWindowChanged()
    {
        return replayWindowChanged;
    }
}
