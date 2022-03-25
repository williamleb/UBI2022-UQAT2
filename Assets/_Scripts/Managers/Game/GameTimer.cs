using Fusion;
using System;
using Systems.Network;

public class GameTimer : NetworkBehaviour
{
    public event Action OnTimerStart;
    public event Action OnTimerPause;
    public event Action OnTimerResume;
    public event Action OnTimerReset;
    public event Action OnTimerExpired;

    [Networked] private TickTimer timer { get; set; }
    [Networked] private int initialDuration { get; set; }

    private bool paused { get; set; }
    private float remainingTimeOnPause;

    public int InitialDuration => initialDuration;
    public float RemainingTime => timer.RemainingTime(Runner) ?? default(float);
    

    //Initialise or reset timer
    public void Init(int duration)
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        initialDuration = duration;

        if (timer.IsRunning)
        {
            Reset();
            return;
        }

        timer = TickTimer.CreateFromSeconds(Runner, duration);
        OnTimerStart?.Invoke();
    }
    public void Pause()
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        remainingTimeOnPause = timer.RemainingTime(Runner) ?? default(float);
        paused = true;
        OnTimerPause?.Invoke();
    }

    public void Resume()
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        timer = TickTimer.CreateFromSeconds(Runner, remainingTimeOnPause);
        paused = false;
        OnTimerResume?.Invoke();
    }

    public void Reset()
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        timer = TickTimer.CreateFromSeconds(Runner, initialDuration);
        OnTimerReset?.Invoke();
    }

    public override void FixedUpdateNetwork()
    {
        if (!NetworkSystem.Instance.IsHost)
            return;

        if (timer.Expired(Runner) && !paused)
        {
            OnTimerExpired?.Invoke();
        }
    }
}
