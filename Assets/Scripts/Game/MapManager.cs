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

        public MapData GetMapById(int id)
        {
            return defaultMaps.Find(map => map.id == id);
        }

        public void LoadMap(MapData map)
        {
            CurrentMap = map;
            SceneManager.LoadScene(map.name);
        }
    }
}