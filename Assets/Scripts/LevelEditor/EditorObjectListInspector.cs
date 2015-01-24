using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(EditorObjectList))]
public class EditorObjectListInspector : Editor
{
	public override void OnInspectorGUI()
	{
		EditorObjectList myTarget = (EditorObjectList)target;

		//If the group list is null, create one
		if(myTarget.objectGroups == null)
		{
			myTarget.objectGroups = new List<GameObjectGroup>();
		}

		//Add a new GameObjectGroup
		if(GUILayout.Button("New Group"))
		{
			myTarget.objectGroups.Add(new GameObjectGroup("new"));
		}

		bool doRemoveGroup = false;
		GameObjectGroup removeGroup = null;

		//Go through each group
		for(int i = 0; i < myTarget.objectGroups.Count; i++)
		{
			EditorGUI.indentLevel++;

			GUILayout.BeginHorizontal();

			//Display group name
			myTarget.objectGroups[i].name = EditorGUILayout.TextField(myTarget.objectGroups[i].name);

			if(GUILayout.Button("Add Item"))
			{
				myTarget.objectGroups[i].objects.Add(null);
			}

			if(GUILayout.Button("Remove Group"))
			{
				doRemoveGroup = true;
				removeGroup = myTarget.objectGroups[i];
			}

			GUILayout.EndHorizontal();

			bool doRemoveItem = false;
			GameObject removeItem = null;

			//Go through each item in the group
			for(int j = 0; j < myTarget.objectGroups[i].objects.Count; j++)
			{
				EditorGUI.indentLevel++;
				GUILayout.BeginHorizontal();

				myTarget.objectGroups[i].objects[j] = (GameObject)EditorGUILayout.ObjectField(myTarget.objectGroups[i].objects[j], typeof(GameObject), false);
				if(GUILayout.Button("X", GUILayout.Width(30f)))
				{
					doRemoveItem = true;
					removeItem = myTarget.objectGroups[i].objects[j];
				}
			
				GUILayout.EndHorizontal();
				EditorGUI.indentLevel--;
			}

			if(doRemoveItem)
			{
				myTarget.objectGroups[i].objects.Remove(removeItem);
			}

			EditorGUI.indentLevel--;
		}

		if(doRemoveGroup)
		{
			myTarget.objectGroups.Remove(removeGroup);
		}

		//Make sure unity saves stuff
		if(GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
}
