using System.Collections.Generic;
using UnityEngine;

namespace LevelEditor
{
    public class BlockInfo
    {
        private Dictionary<string,BlockProperties> blocks;

        public BlockInfo(TextAsset file)
        {
            blocks = new Dictionary<string,BlockProperties>();

            string[] fileLines = file.text.Split('\n');
            foreach(string line in fileLines)
            {
                BlockProperties b = new BlockProperties(line);
                blocks.Add(b.GetName(), b);
            }
        }

        public Vector3[] GetBlockExtents(string blockName)
        {
            if(blocks.ContainsKey(blockName))
            {
                BlockProperties b = blocks[blockName];
                return b.GetExtents();
            }
            else
            {
                Vector3[] defaultExtents = { Vector3.zero };
                return defaultExtents;
            }
        }
    }
}