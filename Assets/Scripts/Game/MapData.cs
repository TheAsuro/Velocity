using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class MapData
    {
        // TODO @HACK use guids
        public int id;
        public string name;
        public string author;
        public Texture2D previewImage;
    }
}