using UnityEngine;
using System.Collections;

public class RemotePlayer
{
	private NetworkPlayer myPlayer;
	public string name;
	public bool active = false;

	public RemotePlayer(NetworkPlayer player)
	{
		myPlayer = player;
	}

	public NetworkPlayer getNetworkPlayer()
	{
		return myPlayer;
	}
}
