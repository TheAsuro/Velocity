using UnityEngine;
using System.Collections.Generic;

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
		BlockProperties b = blocks[blockName];
		return b.GetExtents();
	}

	public Vector3 GetBlockRotation(string blockName)
	{
		BlockProperties b = blocks[blockName];
		return b.GetRotation();
	}

	public Vector3 GetBlockOffset(string blockName)
	{
		BlockProperties b = blocks[blockName];
		return b.GetOffset();
	}
}