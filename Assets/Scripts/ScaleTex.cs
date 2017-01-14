using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScaleTex : MonoBehaviour
{
	public float scaleFactor = 1f;
	public List<string> textures = new List<string>(new string[] {"_MainTex"});
	public TextureScaleMode scaleMode = TextureScaleMode.XY;

	public enum TextureScaleMode
	{
		XY,
		XZ,
		YZ
	}

    private void Awake()
	{
		Vector2 scale;
		if(scaleMode == TextureScaleMode.XY)
		{
			scale = new Vector2(transform.lossyScale.x * scaleFactor, transform.lossyScale.y * scaleFactor);
		}
		else if(scaleMode == TextureScaleMode.XZ)
		{
			scale = new Vector2(transform.lossyScale.x * scaleFactor, transform.lossyScale.z * scaleFactor);
		}
		else
		{
			scale = new Vector2(transform.lossyScale.y * scaleFactor, transform.lossyScale.z * scaleFactor);
		}
		
		foreach(string texture in textures)
		{
			GetComponent<Renderer>().material.GetTexture(texture).wrapMode = TextureWrapMode.Repeat;
			GetComponent<Renderer>().material.SetTextureScale(texture, scale);
		}
	}
}
