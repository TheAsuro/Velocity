using UnityEngine;

public class InfoText : MonoBehaviour
{
	public string content;
	public int boxMargin;
	public bool ignoreDisabledHelp = false;

	private bool display = false;

    private void OnTriggerEnter(Collider col)
	{
		if(col.gameObject.tag.Equals("Player"))
		{
			display = true;
		}
	}

    private void OnTriggerExit(Collider col)
	{
		if(col.gameObject.tag.Equals("Player"))
		{
			display = false;
		}
	}

    private void OnGUI()
	{
		if(display)
		{
			GUILayout.BeginArea(new Rect(Screen.width / 2f - 200f, Screen.height - 50f - boxMargin, 400f, 50f));
			GUILayout.Box(content);
			GUILayout.EndArea();
		}
	}
}
