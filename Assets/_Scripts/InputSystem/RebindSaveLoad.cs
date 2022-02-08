using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    public class RebindSaveLoad : MonoBehaviour
    {
        //TODO change the save mechanic to save to a file
        
        public void LoadOverrides(InputActionAsset actions)
        {
            string rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
                actions.LoadBindingOverridesFromJson(rebinds);
        }

        public void SaveOverrides(InputActionAsset actions)
        {
            string rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("rebinds", rebinds);
        }
    }
}
