using System;
using Fusion;

namespace Managers.Game
{
    public class Score : NetworkBehaviour
    {
        public event Action<int, PlayerRef> OnScoreChanged;

        [Networked(OnChanged = nameof(OnValueChanged))] public int Value { get; private set; }

        public override void Spawned()
        {
            Value = 0;

            gameObject.name = $"Score-Player{Object.InputAuthority.PlayerId}";
            
            if (GameManager.Instance)
                GameManager.Instance.RegisterScore(this, Object.InputAuthority);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (GameManager.HasInstance)
                GameManager.Instance.UnregisterScore(Object.InputAuthority);
        }

        public void Add(int scoreToAdd)
        {
            Value += scoreToAdd;
        }

        public void Remove(int scoreToRemove)
        {
            Value -= scoreToRemove;
        }

        public void Reset()
        {
            Value = 0;
        }
        
        private static void OnValueChanged(Changed<Score> changed)
        {
            var score = changed.Behaviour;
            score.OnScoreChanged?.Invoke(score.Value, score.Object.InputAuthority);
        }
    }
}