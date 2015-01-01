using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

[XmlRoot("LevelData")]
public class LevelData
{
	[XmlArray("LevelObjects")]
	[XmlArrayItem("ObjectData")]
	public List<ObjectData> levelObjects;

	public void WriteToFile(string path)
	{
		XmlSerializer serializer = new XmlSerializer(typeof(LevelData));
		FileStream stream = new FileStream(path, FileMode.Create);
		serializer.Serialize(stream, this);
	}
}