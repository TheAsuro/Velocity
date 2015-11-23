using UnityEngine;
using System.Collections;
using Api;

public class Test : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        Account acc = new Account("ayyidnb");
        acc.OnLoginFinished += (obj, e) => Debug.Log(e.Content);
        acc.StartLogin("lmaoxdxd");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
