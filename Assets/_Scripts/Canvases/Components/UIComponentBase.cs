using UnityEngine;
using Utilities.Extensions;

namespace Canvases.Components
{
    public class UIComponentBase : MonoBehaviour
    {

        public void Show() => gameObject.Show();

        public void Hide() => gameObject.Hide();

        public void ToggleDisplay() => gameObject.SetActive(!gameObject.activeInHierarchy);

        public void SetActive(bool active) => gameObject.SetActive(active);
    }
}