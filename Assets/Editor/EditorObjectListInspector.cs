using UnityEditor;
using UnityEngine;
using System.Collections;

public class EditorObjectListInspector : Editor
{
	public override void OnInspectorGUI()
	{
		EditorObjectList myTarget = (EditorObjectList)target;

		if(GUILayout.Button("NEW"))
		{
			myTarget.objectGroups.Add(new GameObjectGroup("new"));
		}

		for(int i = 0; i < myTarget.objectGroups.Count; i++)
		{
			myTarget.objectGroups[i].name = EditorGUILayout.TextField(myTarget.objectGroups[i].name);
		}
	}
}
