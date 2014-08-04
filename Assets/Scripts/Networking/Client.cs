using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
	public GameObject ghostPrefab;

	private NetworkView view;
	private bool connected = false;
	private bool sendPosition = false;
	private Vector3 lastPosition = Vector3.zero;
	private Dictionary<string,GameObject> ghostList;

	void Start()
	{
		ghostList = new Dictionary<string,GameObject>();
		view = GetComponent<NetworkView>();
		view.observed = this;
	}

	public void ConnectToServer(string ip, int port, string password)
	{
		//Set the network view to observe this script
		view.observed = this;

		//Connect to server
		NetworkConnectionError error = Network.Connect(ip, port, password);
		GameInfo.info.writeToConsole("Trying to connect to server...");
		if(error != NetworkConnectionError.NoError)
		{
			GameInfo.info.writeToConsole("Failed to connect to server!");
		}
	}

	void OnConnectedToServer()
	{
		//Send a message with your name to the server, so it can assign your name to your GUID
		view.RPC("addPlayer", RPCMode.Server, GameInfo.info.getCurrentSave().getPlayerName());
		connected = true;
	}

	void FixedUpdate()
	{
		if(GameInfo.info.getPlayerObject() != null)
		{
			Vector3 playerPos = GameInfo.info.getPlayerObject().transform.position;
			if(!lastPosition.Equals(playerPos) && sendPosition)
			{
				lastPosition = playerPos;
				view.RPC("setPlayerPosition", RPCMode.Server, playerPos);
			}
		}
	}

	public void DisconnectFromServer()
	{
		connected = false;
		Network.Disconnect();
		GameInfo.info.writeToConsole("Disconnected from server.");
	}

	[RPC]
	public void ChangeLevel(int levelId)
	{
		Application.LoadLevel(levelId);
	}

	[RPC]
	public void StartTransmitPositions()
	{
		sendPosition = true;
	}

	[RPC]
	public void setGhostPosition(Vector3 position, string name)
	{
		if(!ghostList.ContainsKey(name))
		{
			GameObject newGhost = (GameObject)GameObject.Instantiate(ghostPrefab, position, new Quaternion());
			ghostList.Add(name, newGhost);
		}
		GameObject ghost = ghostList[name];
		ghost.transform.position = position;
	}

	void OnLevelWasLoaded(int id)
	{
		if(connected)
		{
			view.RPC("activatePlayer", RPCMode.Server);
		}
	}
}
