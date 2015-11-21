using UnityEngine;
using System.Collections;
using Api;

public class Test : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        Account acc = new Account("ayy");
        acc.StartCreate("lmao", "gay@ayy.de");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
