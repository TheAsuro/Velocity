using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Server : MonoBehaviour
{
	private NetworkView view;
	private List<RemotePlayer> playerList;

	void Start()
	{
		view = GetComponent<NetworkView>();
		view.observed = this;
	}

	public void StartServer(int connections, int port, bool useNat, string password)
	{
		playerList = new List<RemotePlayer>();
		Network.incomingPassword = password;
		Network.InitializeSecurity();
		NetworkConnectionError error = Network.InitializeServer(connections, port, useNat);
		if(error != NetworkConnectionError.NoError)
		{
			GameInfo.info.writeToConsole("Failed to start server!");
		}
	}

	public void StopServer()
	{
		Network.Disconnect();
		GameInfo.info.writeToConsole("Stopped server.");
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		playerList.Add(new RemotePlayer(player));
		GameInfo.info.writeToConsole("Player " + player.guid + " connected.");
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		RemotePlayer rem = getPlayerByNetworkPlayer(player);
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

	//Player has updates his position
	[RPC]
	public void setPlayerPosition(Vector3 playerPos, NetworkMessageInfo info)
	{
		view.RPC("setGhostPosition", RPCMode.All, playerPos, getPlayerByNetworkPlayer(info.sender).name);
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
}
