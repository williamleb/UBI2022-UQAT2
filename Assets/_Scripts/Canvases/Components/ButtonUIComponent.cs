using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if AUDIO_EVENT
using AudioEvent;
#endif

namespace Canvases.Components
{
    public class ButtonUIComponent : UIComponentBase
    {
        [SerializeField] private Button button;

        public Color Color
        {
            set => button.image.color = value;
        }

        public void OnClick(UnityAction callback) => button.onClick.AddListener(callback);
        private void OnDestroy() => button.onClick.RemoveAllListeners();
        
        private void OnValidate() => button = GetComponent<Button>();

#if AUDIO_EVENT
        [SerializeField] private SimpleAudioEvent _clickSound;
        [SerializeField] private AudioSource _audioSource;

        private void Awake() => _button.onClick.AddListener(() => _clickSound.Play(_audioSource));
#endif
        
    }
}