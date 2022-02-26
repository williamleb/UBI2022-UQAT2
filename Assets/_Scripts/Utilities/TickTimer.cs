using System;

namespace Utilities
{
    public class TickTimer
    {
        public event Action OnTimerEnd;
        
        public float RemainingSeconds { get; private set; }

        private float maxDuration;

        public TickTimer(float duration)
        {
            maxDuration = duration;
            Reset();
        }

        public void Set(float newTime)
        {
            maxDuration = newTime;
            Reset();
        }

        public void Cancel()
        {
            RemainingSeconds = 0;
        }

        public void Reset()
        {
            RemainingSeconds = maxDuration;
        }

        public void Tick(float deltaTime)
        {
            if (RemainingSeconds == 0f) return;
            RemainingSeconds -= deltaTime;
            CheckForTimerEnd();
        }

        private void CheckForTimerEnd()
        {
            if (RemainingSeconds > 0) return;

            RemainingSeconds = 0;
            
            OnTimerEnd?.Invoke();
        }
    }
}