using Trisibo;
using UnityEngine;

namespace Scriptables
{
    [CreateAssetMenu(fileName = "Scenes", menuName = "Game/Scenes")]
    public class GameScenes : ScriptableObject
    {
        [SerializeField] private SceneField mainMenu;
        [SerializeField] private SceneField lobby;
        [SerializeField] private SceneField game;

        public SceneField MainMenu => mainMenu;
        public SceneField Lobby => lobby;
        public SceneField Game => game;
    }
}