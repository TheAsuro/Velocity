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

	public string getName()
	{
		return soundName;
	}

	public AudioClip getClip()
	{
		return clip;
	}
}
