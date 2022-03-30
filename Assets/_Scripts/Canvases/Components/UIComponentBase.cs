using System;
using UnityEngine;
using Utilities.Extensions;

namespace Canvases.Components
{
    // ReSharper disable once InconsistentNaming Reason: UI should be capitalized
    public class UIComponentBase : MonoBehaviour
    {
        public void Show() => gameObject.Show();

        public void Hide() => gameObject.Hide();

        public void ToggleDisplay() => gameObject.SetActive(!gameObject.activeInHierarchy);

        public void SetActive(bool active) => gameObject.SetActive(active);
    }
}