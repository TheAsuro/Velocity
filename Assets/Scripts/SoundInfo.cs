using UnityEngine;
using System.Collections;

//...
public class SoundInfo
{
	public string soundName;
	public AudioClip clip;

	public SoundInfo(string pName, AudioClip pClip)
	{
		soundName = pName;
		clip = pClip;
	}

	public string GetName()
	{
		return soundName;
	}

	public AudioClip GetClip()
	{
		return clip;
	}
}
