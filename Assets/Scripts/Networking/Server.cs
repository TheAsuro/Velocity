using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Server : MonoBehaviour
{
	private NetworkView view;
	private Client myClient;
	private List<RemotePlayer> playerList;
	private bool running = false;
	private int myPort = -1;

	void Start()
	{
		view = GetComponent<NetworkView>();
		myClient = GetComponent<Client>();
		view.observed = this;
	}

	public void StartServer(int connections, int port, string password)
	{
		myPort = port;
		playerList = new List<RemotePlayer>();
		Network.incomingPassword = password;
		Network.InitializeSecurity();
		NetworkConnectionError error = Network.InitializeServer(connections, port, !Network.HavePublicAddress());
		if(error != NetworkConnectionError.NoError)
		{
			GameInfo.info.writeToConsole("Failed to start server!");
		}
	}

	public void StopServer()
	{
		running = false;
		Network.Disconnect();
		GameInfo.info.writeToConsole("Stopped server.");
	}

	void OnServerInitialized()
	{
		running = true;
		myClient.JoinServer(true);
		GameInfo.info.writeToConsole("Server initzialized on port " + myPort + ".");
	}

	void OnLevelWasLoaded(int id)
	{
		if(running)
		{
			if(!myClient.isConnected())
			{
				myClient.JoinServer(true);
			}
			view.RPC("ChangeLevel", RPCMode.Others, id);
		}
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		playerList.Add(new RemotePlayer(player));
		GameInfo.info.writeToConsole("Player " + player.guid + " connected.");
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		RemotePlayer rem = getPlayerByNetworkPlayer(player);
		myClient.RemovePlayer(player.guid);
		view.RPC("RemovePlayer", RPCMode.Others, player.guid);
		playerList.Remove(rem);
		GameInfo.info.writeToConsole("Player " + rem.name + " disconnected.");
	}

	//Recieve a name from a client and assign it to the correct player
	[RPC]
	public void addPlayer(string name, NetworkMessageInfo info)
	{
		getPlayerByNetworkPlayer(info.sender).name = name;
		GameInfo.info.writeToConsole("Player " + info.sender.guid + " has the name '" + name + "'.");

		//Send the player the current level name
		view.RPC("ChangeLevel", info.sender, Application.loadedLevel);
	}

	public void addLocalPlayer(string name)
	{
		playerList.Add(new RemotePlayer(name));
		activateLocalPlayer();
		myClient.StartTransmitPositions();
		GameInfo.info.writeToConsole("Local player " + name + " connected.");
	}

	//Player finished loading the level
	[RPC]
	public void activatePlayer(NetworkMessageInfo info)
	{
		//Activate the player on the server
		RemotePlayer player = getPlayerByNetworkPlayer(info.sender);
		player.active = true;
		GameInfo.info.writeToConsole("Player " + player.name + " has loaded!");

		//Send confirmation
		view.RPC("StartTransmitPositions", info.sender);
	}

	public void activateLocalPlayer()
	{
		RemotePlayer player = getLocalPlayer();
		player.active = true;
		GameInfo.info.writeToConsole("(Local) Player " + player.name + " has loaded!");
	}

	//Player has updates his position
	[RPC]
	public void setPlayerPosition(Vector3 playerPos, NetworkMessageInfo info)
	{
		view.RPC("setGhostPosition", RPCMode.All, playerPos, info.sender.guid);
	}

	public void setLocalPlayerPosition(Vector3 playerPos)
	{
		view.RPC("setGhostPosition", RPCMode.All, playerPos, getLocalPlayer().name);
	}

	private RemotePlayer getPlayerByNetworkPlayer(NetworkPlayer netPlayer)
	{
		foreach(RemotePlayer player in playerList)
		{
			if(player.getNetworkPlayer() == netPlayer)
			{
				return player;
			}
		}
		GameInfo.info.writeToConsole("Unlisted player!");
		return null;
	}

	private RemotePlayer getLocalPlayer()
	{
		foreach(RemotePlayer player in playerList)
		{
			if(player.isLocal())
			{
				return player;
			}
		}
		return null;
	}

	public bool isRunning()
	{
		return running;
	}
}
