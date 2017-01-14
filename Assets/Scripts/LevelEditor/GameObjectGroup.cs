using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    [System.Serializable]
    public class GameObjectGroup
    {
        public string name;
        public List<GameObject> objects;

        public GameObjectGroup(string pName)
        {
            name = pName;
            objects = new List<GameObject>();
        }

        public GameObjectGroup(string pName, List<GameObject> pObjects)
        {
            name = pName;
            objects = pObjects;
        }
    }
}
