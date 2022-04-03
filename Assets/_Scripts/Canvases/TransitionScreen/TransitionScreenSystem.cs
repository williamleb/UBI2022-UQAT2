using System.Linq;
using Scriptables;
using UnityEngine;
using Utilities.Singleton;

namespace Canvases.TransitionScreen
{
    public class TransitionScreenSystem : PersistentSingleton<TransitionScreenSystem>
    {
        private const string DEFAULT_TEXT = "LOADING";
        
        private const string PREFABS_FOLDER_PATH = "Game";
        
        private GamePrefabs prefabs;
        private TransitionScreen transitionScreen;

        public bool IsShown => transitionScreen && transitionScreen.IsShown;
        public bool IsHidden => transitionScreen == null || transitionScreen.IsHidden;
        
        protected override void Awake()
        {
            base.Awake();
            LoadPrefabs();
            SpawnTransitionScreen();
        }

        private void LoadPrefabs()
        {
            var prefabResources = Resources.LoadAll<GamePrefabs>(PREFABS_FOLDER_PATH);

            Debug.Assert(prefabResources.Any(),
                $"An object of type {nameof(GamePrefabs)} should be in the folder {PREFABS_FOLDER_PATH}");
            if (prefabResources.Length > 1)
                Debug.LogWarning(
                    $"More than one object of type {nameof(GamePrefabs)} was found in the folder {PREFABS_FOLDER_PATH}. Taking the first one.");

            prefabs = prefabResources.First();
        }

        private void SpawnTransitionScreen()
        {
            var transitionScreenObject = Instantiate(prefabs.TransitionScreenPrefab, transform);
            
            transitionScreen = transitionScreenObject.GetComponent<TransitionScreen>();
            Debug.Assert(transitionScreen, $"Could not find script {nameof(TransitionScreen)} in the transition screen prefab.");
        }

        public void Show(string text = DEFAULT_TEXT)
        {
            if (transitionScreen)
            {
                transitionScreen.ChangeText(text);
                transitionScreen.Show();
            }
        }
        
        public void Hide()
        {
            if (transitionScreen)
                transitionScreen.Hide();
        }
    }
}