using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace LevelEditor
{
    [XmlRoot("LevelData")]
    public class LevelData
    {
        [XmlArray("LevelObjects")]
        [XmlArrayItem("ObjectData")]
        public List<ObjectData> levelObjects;

        public static LevelData CreateFromFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LevelData));
            FileStream stream = new FileStream(path, FileMode.Open);
            LevelData returnData = (LevelData)serializer.Deserialize(stream);
            stream.Dispose();
            return returnData;
        }

        public void WriteToFile(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LevelData));
            FileStream stream = new FileStream(path, FileMode.Create);
            serializer.Serialize(stream, this);
            stream.Dispose();
        }
    }
}