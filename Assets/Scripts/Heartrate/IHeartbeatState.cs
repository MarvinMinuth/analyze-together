using UnityEngine;

public interface IHeartbeatState
{
    void Enter(HeartrateCoordinator coordinator);
    void Update(HeartrateCoordinator coordinator);
}

public class LubState : IHeartbeatState
{
    private float startTime;

    public void Enter(HeartrateCoordinator coordinator)
    {
        startTime = Time.time;
        if (coordinator.IsHapticFeedbackActivated())
        {
            coordinator.hapticFeedback.TriggerHaptic();
        }
        if (coordinator.IsAudioFeedbackActivated())
        {
            coordinator.audioFeedback.PlayLub();
        }

    }
    public void Update(HeartrateCoordinator coordinator)
    {
        if (Time.time - startTime > coordinator.CurrentLubLength)
        {
            coordinator.SetState(new ShortPauseState());
        }
    }
}

public class DubState : IHeartbeatState
{
    private float startTime;

    public void Enter(HeartrateCoordinator coordinator)
    {
        startTime = Time.time;
        if (coordinator.IsHapticFeedbackActivated())
        {
            coordinator.hapticFeedback.TriggerHaptic();
        }
        if (coordinator.IsAudioFeedbackActivated())
        {
            coordinator.audioFeedback.PlayDub();
        }

    }
    public void Update(HeartrateCoordinator coordinator)
    {
        if (Time.time - startTime < coordinator.CurrentDubLength)
        {
            coordinator.SetState(new LongPauseState());
        }
    }
}

public class ShortPauseState : IHeartbeatState
{
    private float startTime;

    public void Enter(HeartrateCoordinator coordinator)
    {
        startTime = Time.time;
    }
    public void Update(HeartrateCoordinator coordinator)
    {
        if (Time.time - startTime > coordinator.CurrentShortPauseLength)
        {
            coordinator.SetState(new DubState());
        }
    }
}

public class LongPauseState : IHeartbeatState
{
    private float startTime;
    public void Enter(HeartrateCoordinator coordinator)
    {
        startTime = Time.time;
    }
    public void Update(HeartrateCoordinator coordinator)
    {
        if (Time.time - startTime > coordinator.CurrentLongPauseLength)
        {
            coordinator.SetState(new LubState());
        }
    }
}

public class IdleHeartbeatState : IHeartbeatState
{
    public void Enter(HeartrateCoordinator coordinator)
    {
    }
    public void Update(HeartrateCoordinator coordinator)
    {
        coordinator.SetState(new LubState());
    }
}

public class WaitingHeartbeatState : IHeartbeatState
{
    public void Enter(HeartrateCoordinator coordinator)
    {
    }

    public void Update(HeartrateCoordinator coordinator)
    {
    }
}