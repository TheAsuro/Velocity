using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Client : MonoBehaviour
{
	public GameObject ghostPrefab;

	private NetworkView view;
	private bool connected = false;
	private bool localConnection = false;
	private bool sendPosition = false;
	private Vector3 lastPosition = Vector3.zero;
	private Dictionary<string,GameObject> ghostList;
	private Server myServer;

	void Start()
	{
		ghostList = new Dictionary<string,GameObject>();
		view = GetComponent<NetworkView>();
		myServer = GetComponent<Server>();
		view.observed = this;
	}

	//Tries to create a p2p connection to a server
	public void ConnectToServer(string ip, int port, string password)
	{
		//Connect to server
		GameInfo.info.writeToConsole("Trying to connect to server...");
		NetworkConnectionError error = Network.Connect(ip, port, password);
		
		if(error != NetworkConnectionError.NoError)
		{
			GameInfo.info.writeToConsole("Failed to connect to server!");
		}
	}

	//p2p connection established
	void OnConnectedToServer()
	{
		JoinServer(false);
	}

	//Disconnected for whatever reason
	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		GameInfo.info.writeToConsole("Lost Connection!");
		connected = false;
		clearGhosts();
	}

	//Send the server a message that we loaded
	void OnLevelWasLoaded(int id)
	{
		if(connected && !localConnection)
		{
			sendMessageToServer("activatePlayer");
		}
	}

	//Send the position to create ghosts for other players
	void FixedUpdate()
	{
		if(GameInfo.info.getPlayerInfo() != null)
		{
			Vector3 playerPos = GameInfo.info.getPlayerInfo().getPosition();
			if(!lastPosition.Equals(playerPos) && sendPosition)
			{
				lastPosition = playerPos;
				if(localConnection)
				{
					myServer.setLocalPlayerPosition(playerPos);
				}
				else
				{
					sendMessageToServer("setPlayerPosition", playerPos);
				}
			}
		}
	}

	//Send the server a message that we connected and want more info
	public void JoinServer(bool local)
	{
		//We can't join without a player name
		if(GameInfo.info.getCurrentSave() != null)
		{
			if(local)
			{
				myServer.addLocalPlayer(GameInfo.info.getCurrentSave().getPlayerName());
			}
			else
			{
				//Send a message with your name to the server, so it can assign your name to your GUID
				sendMessageToServer("addPlayer", GameInfo.info.getCurrentSave().getPlayerName());
			}
			localConnection = local;
			connected = true;
		}
		else
		{
			GameInfo.info.writeToConsole("Can't connect without a loaded save!");
		}
	}

	private void sendMessageToServer(string procedure, params object[] parameters)
	{
		view.RPC(procedure, RPCMode.Server, parameters);
	}

	public void DisconnectFromServer()
	{
		connected = false;
		Network.Disconnect();
		clearGhosts();
		GameInfo.info.writeToConsole("Disconnected from server.");
	}

	private void clearGhosts()
	{
		foreach(KeyValuePair<string,GameObject> pair in ghostList)
		{
			GameObject.Destroy(pair.Value);
		}
		ghostList.Clear();
	}

	//Server changed level/Sends level for the first time
	[RPC]
	public void ChangeLevel(int levelId)
	{
		Application.LoadLevel(levelId);
	}

	//Server wants us to send positions for ghosts
	[RPC]
	public void StartTransmitPositions()
	{
		sendPosition = true;
	}

	//Server doesn't want our position
	[RPC]
	public void StopTransmitPositions()
	{
		sendPosition = false;
	}

	//position of a ghost
	[RPC]
	public void setGhostPosition(Vector3 position, string guid)
	{
		if(!ghostList.ContainsKey(guid))
		{
			GameObject newGhost = (GameObject)GameObject.Instantiate(ghostPrefab, position, new Quaternion());
			ghostList.Add(guid, newGhost);
		}
		GameObject ghost = ghostList[guid];
		ghost.transform.position = position;
	}

	[RPC]
	public void RemovePlayer(string guid)
	{
		foreach(KeyValuePair<string,GameObject> pair in ghostList)
		{
			if(pair.Key == guid)
			{
				GameObject.Destroy(pair.Value);
				ghostList.Remove(pair.Key);
				break;
			}
		}
	}

	public bool isConnected()
	{
		return connected;
	}
}
