using UnityEngine;

public class BlockProperties
{
	private string name;
	private Vector3[] extents;
	private Vector3 rotation;
	private Vector3 offset;

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

	public BlockProperties(string pName, Vector3[] pExtents, Vector3 pRotation, Vector3 pOffset)
	{
		name = pName;
		extents = pExtents;
		rotation = pRotation;
		offset = pOffset;
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
			case "rotation":
				SetRotation(propertyContent);
				break;
			case "offset":
				SetOffset(propertyContent);
				break;
			default:
				Debug.Log("rip: " + propertyName);
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

	private void SetRotation(string rotDesc)
	{
		string[] rotValues = rotDesc.Split(',');
		rotation = new Vector3(float.Parse(rotValues[0]), float.Parse(rotValues[1]), float.Parse(rotValues[2]));
	}

	private void SetOffset(string offsetDesc)
	{
		string[] offsetValues = offsetDesc.Split(',');
		offset = new Vector3(float.Parse(offsetValues[0]), float.Parse(offsetValues[1]), float.Parse(offsetValues[2]));
	}

	public string GetName()
	{
		return name;
	}

	public Vector3[] GetExtents()
	{
		return extents;
	}

	public Vector3 GetRotation()
	{
		return rotation;
	}

	public Vector3 GetOffset()
	{
		return offset;
	}
}