using UnityEngine;

namespace Canvases.Menu.Main
{
    public class ShowMainMenuOnStart : MonoBehaviour
    {
        private void Start()
        {
            if (MenuManager.HasInstance)
            {
                MenuManager.Instance.ShowMenu(MenuManager.Menu.Main);
            }
        }
    }
}