using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScaleTex : MonoBehaviour
{
	public float scaleFactor = 1f;
	public List<string> textures = new List<string>(new string[] {"_MainTex"});
	public textureScaleMode scaleMode = textureScaleMode.XY;

	public enum textureScaleMode
	{
		XY,
		XZ,
		YZ
	}

	void Awake()
	{
		Vector2 scale;
		if(scaleMode == textureScaleMode.XY)
		{
			scale = new Vector2(transform.lossyScale.x * scaleFactor, transform.lossyScale.y * scaleFactor);
		}
		else if(scaleMode == textureScaleMode.XZ)
		{
			scale = new Vector2(transform.lossyScale.x * scaleFactor, transform.lossyScale.z * scaleFactor);
		}
		else
		{
			scale = new Vector2(transform.lossyScale.y * scaleFactor, transform.lossyScale.z * scaleFactor);
		}
		
		foreach(string texture in textures)
		{
			renderer.material.GetTexture(texture).wrapMode = TextureWrapMode.Repeat;
			renderer.material.SetTextureScale(texture, scale);
		}
	}
}
