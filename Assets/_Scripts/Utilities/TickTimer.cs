using System;

namespace Utilities
{
    public class TickTimer
    {
        public event Action OnTimerEnd;

        private float remainingSeconds;

        private float maxDuration;

        public TickTimer(float duration, bool startImmediately = false)
        {
            maxDuration = duration;
            if (startImmediately)
            {
                Reset();
            }
            else
            {
                remainingSeconds = 0;
            }
        }

        public void Set(float newTime)
        {
            maxDuration = newTime;
            Reset();
        }

        public void Cancel()
        {
            remainingSeconds = 0;
        }

        public void Reset()
        {
            remainingSeconds = maxDuration;
        }

        public void Tick(float deltaTime)
        {
            if (remainingSeconds == 0f) return;
            remainingSeconds -= deltaTime;
            CheckForTimerEnd();
        }

        private void CheckForTimerEnd()
        {
            if (remainingSeconds > 0) return;

            remainingSeconds = 0;
            
            OnTimerEnd?.Invoke();
        }
    }
}