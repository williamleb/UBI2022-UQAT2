using Systems.Settings;
using UnityEngine;

namespace Units.Customization
{
    public class AICustomization : CustomizationBase
    {
        public void LocalRandomize()
        {
            Settings = SettingsSystem.CustomizationSettings;

            Head = Random.Range(0, Settings.NumberOfHeadElements);
            HairColor = Random.Range(0, Settings.NumberOfHairColors);
            Eyes = Random.Range(0, Settings.NumberOfEyeElements);
            Skin = Random.Range(0, Settings.NumberOfSkinElements);
            
            UpdateAll();
        }
    }
}