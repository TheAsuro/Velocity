using UnityEngine;
using System.Collections;
using Api;
using Settings;

public class Test : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        Settings.Input.BindKey(-1, bla);
	}
	
	// Update is called once per frame
	void Update () {
        Settings.Input.ExecuteBoundActions();
	}

    void bla()
    {
        Account acc = new Account("ayyidnb");
        acc.OnLoginFinished += (obj, e) => Debug.Log(e.Content);
        acc.StartLogin("lmaoxdxd");
    }
}
