using UnityEngine;
using System.Collections;

public class RemotePlayer
{
	private NetworkPlayer myPlayer;
	private bool localPlayer = false;
	public string name;
	public bool active = false;

	//Creates a new local player
	public RemotePlayer(string pName)
	{
		localPlayer = true;
		name = pName;
	}

	//Creates a new network player
	public RemotePlayer(NetworkPlayer player)
	{
		myPlayer = player;
	}

	public NetworkPlayer getNetworkPlayer()
	{
		return myPlayer;
	}

	public bool isLocal()
	{
		return localPlayer;
	}
}
