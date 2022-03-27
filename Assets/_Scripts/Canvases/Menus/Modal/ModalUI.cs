using System.Collections;
using Canvases.Components;
using Units.Player;
using UnityEngine;
using Utilities;

namespace Canvases.Menu.Modal
{
    public class ModalUI : AbstractMenu
    {
        [SerializeField] private TextUIComponent header;
        [SerializeField] private TextUIComponent text;
        [SerializeField] private ButtonUIComponent closeButton;
        
        protected override EntryDirection EnterDirection => EntryDirection.Down;
        protected override EntryDirection LeaveDirection => EntryDirection.Down;

        private Coroutine showCoroutine;

        public bool IsShown => showCoroutine != null;

        protected override void OnEnable()
        {
            base.OnEnable();
            closeButton.OnClick += HideModal;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            closeButton.OnClick -= HideModal;

            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }
        }

        protected override bool ShowImplementation()
        {
            if (IsShown)
                return false;
            
            return base.ShowImplementation();
        }

        protected override bool ShowForImplementation(PlayerEntity playerEntity)
        {
            if (IsShown)
                return false;
            
            return base.ShowForImplementation(playerEntity);
        }

        public bool Show(string textString, string headerString, float numberOfSeconds = 5f)
        {
            if (IsShown)
                return false;
            
            text.Text = textString;
            header.Text = headerString.ToUpper();
            showCoroutine = StartCoroutine(ShowRoutine(numberOfSeconds));
            return true;
        }

        private IEnumerator ShowRoutine(float numberOfSeconds)
        {
            Show();
            yield return Helpers.GetWait(numberOfSeconds);
            Hide();
            showCoroutine = null;
        }

        private void HideModal()
        {
            Hide();
        }
    }
}