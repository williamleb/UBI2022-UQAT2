using System;
using Fusion;

namespace Managers.Score
{
    public class Score : NetworkBehaviour
    {
        public event Action<int> OnScoreChanged;

        [Networked(OnChanged = nameof(OnValueChanged))] public int Value { get; private set; }

        public override void Spawned()
        {
            Value = 0;

            gameObject.name = $"Score-Player{Object.InputAuthority.PlayerId}";
            
            if (ScoreManager.Instance)
                ScoreManager.Instance.RegisterScore(this, Object.InputAuthority);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (ScoreManager.HasInstance)
                ScoreManager.Instance.UnregisterScore(Object.InputAuthority);
        }

        public void Add(int scoreToAdd)
        {
            Value += scoreToAdd;
        }

        public void Remove(int scoreToRemove)
        {
            Value = Math.Max(0, Value - scoreToRemove);
        }

        public void Reset()
        {
            Value = 0;
        }
        
        private static void OnValueChanged(Changed<Score> changed)
        {
            var score = changed.Behaviour;
            score.OnScoreChanged?.Invoke(score.Value);
        }
    }
}