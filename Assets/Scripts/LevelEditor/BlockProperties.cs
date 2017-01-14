using UnityEngine;

namespace LevelEditor
{
    public class BlockProperties
    {
        private string name;
        private Vector3[] extents;

        public BlockProperties(string description)
        {
            //Remove all spaces
            string shortDesc = description.Replace(" ", "");
		
            //Read name
            int eqIndex = shortDesc.IndexOf("=");
            name = shortDesc.Substring(0, eqIndex);

            //Read properties, separated by ";"
            string rest = shortDesc.Substring(eqIndex + 1);
            while(!rest.Equals(""))
            {
                if(rest.Contains(";"))
                {
                    ReadProperty(rest.Substring(0, rest.IndexOf(";")));
                    rest = rest.Substring(rest.IndexOf(";") + 1);
                }
                else
                {
                    ReadProperty(rest);
                    rest = "";
                }
            }
        }

        public BlockProperties(string pName, Vector3[] pExtents)
        {
            name = pName;
            extents = pExtents;
        }

        private void ReadProperty(string propertyDesc)
        {
            int colonIndex = propertyDesc.IndexOf(":");
            string propertyName = propertyDesc.Substring(0, colonIndex);
            string propertyContent = propertyDesc.Substring(colonIndex + 1);

            switch(propertyName.ToLower())
            {
                case "extent":
                    SetExtents(propertyContent);
                    break;
                default:
                    Debug.Log("Unknown property name: " + propertyName);
                    break;
            }
        }

        private void SetExtents(string extDesc)
        {
            string[] strVectors = extDesc.Split('|');
            Vector3[] vectors = new Vector3[strVectors.Length];

            for(int i = 0; i < strVectors.Length; i++)
            {
                string[] values = strVectors[i].Split(',');
                vectors[i] = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
            }

            extents = vectors;
        }

        public string GetName()
        {
            return name;
        }

        public Vector3[] GetExtents()
        {
            return extents;
        }
    }
}