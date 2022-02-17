using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    public static class RebindSaveLoad
    {
        public static void LoadOverrides(InputActionAsset actions)
        {
            string rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
                actions.LoadBindingOverridesFromJson(rebinds);
        }

        public static void SaveOverrides(InputActionAsset actions)
        {
            string rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        }
    }
}