using UnityEngine;
using System.Collections;

public class Server : MonoBehaviour
{
	public void StartServer(string password, int connections, int port, bool useNat)
	{
		Network.incomingPassword = password;
		Network.InitializeSecurity();
		Network.InitializeServer(connections, port, useNat);
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		GameInfo.info.writeToConsole("Player " + player.guid + " connected.");
	}
}
