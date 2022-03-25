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
            closeButton.OnClick += Hide;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            closeButton.OnClick -= Hide;

            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
                showCoroutine = null;
            }
        }

        public override void Show()
        {
            if (IsShown)
                return;
            
            base.Show();
        }

        public override void ShowFor(PlayerEntity playerEntity)
        {
            if (IsShown)
                return;
            
            base.ShowFor(playerEntity);
        }

        public void Show(string textString, string headerString, float numberOfSeconds = 5f)
        {
            if (IsShown)
                return;
            
            text.Text = textString;
            header.Text = headerString.ToUpper();
            showCoroutine = StartCoroutine(ShowRoutine(numberOfSeconds));
        }

        private IEnumerator ShowRoutine(float numberOfSeconds)
        {
            Show();
            yield return Helpers.GetWait(numberOfSeconds);
            Hide();
            showCoroutine = null;
        }
    }
}