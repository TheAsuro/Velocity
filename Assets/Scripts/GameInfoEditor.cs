using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

//F U C K T H I S
public class GameInfoEditor : Editor
{/*
	public override void OnInspectorGUI()
	{
		GameInfo myInfo = (GameInfo)target;

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Skin");
		myInfo.skin = (GUISkin)EditorGUILayout.ObjectField(myInfo.skin, typeof(GUISkin), false);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical();

		List<SoundInfo> tempSounds = new List<SoundInfo>();

		foreach(SoundInfo sInfo in myInfo.sounds)
		{
			EditorGUILayout.BeginHorizontal();
			string soundName = EditorGUILayout.TextField(sInfo.getName());
			AudioClip clip = (AudioClip)EditorGUILayout.ObjectField(sInfo.getClip(), typeof(AudioClip), true);
			SoundInfo tempInfo = new SoundInfo(soundName, clip);
			tempSounds.Add(tempInfo);

			if(GUILayout.Button("Remove"))
			{
				tempSounds.Remove(tempInfo);
			}

			EditorGUILayout.EndHorizontal();
		}

		myInfo.sounds = tempSounds;

		if(GUILayout.Button("Add New"))
		{
			myInfo.sounds.Add(new SoundInfo("", null));
		}

		EditorGUILayout.EndVertical();
	}*/
}
