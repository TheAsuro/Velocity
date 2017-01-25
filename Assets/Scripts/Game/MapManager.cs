using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    [CreateAssetMenu]
    public class MapManager : ScriptableObject
    {
        public MapData CurrentMap { get; private set; }

        [SerializeField] private List<MapData> defaultMaps;

        public List<MapData> DefaultMaps
        {
            get { return defaultMaps; }
        }

        public void LoadMap(MapData map)
        {
            CurrentMap = map;
            SceneManager.sceneLoaded += OnMapLoaded;
            SceneManager.LoadScene(map.name);
        }

        private void OnMapLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnMapLoaded;
            WorldInfo.info.CreatePlayer(false);
        }
    }
}