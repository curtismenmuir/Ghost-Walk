using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour 
{
	[SerializeField] Text connectionText;
	[SerializeField] public Transform[] spawnPoints;
	
	[SerializeField] GameObject ScoreBoard;
	[SerializeField] InputField ScoreBox;
	[SerializeField] GameObject serverWindow;
	[SerializeField] GameObject nameWindow;
	[SerializeField] InputField username;
	[SerializeField] InputField roomName;
	[SerializeField] InputField roomList;
	[SerializeField] GameObject MessageBox;
	[SerializeField] InputField messageWindow;
	[SerializeField] Camera playerCam;
	[SerializeField] Camera roomCam;
	[SerializeField] GameObject healthPanel;
	[SerializeField] InputField healthBox;
	[SerializeField] GameObject InstructionPanel;

	GameObject player;
	Queue<string> messages;
	const int mesCount = 4;
	PhotonView photonView;
	bool firstLife = true;


	void Start () 
	{
		playerCam.enabled = false;
		MessageBox.SetActive(false);
		photonView = GetComponent<PhotonView> ();
		messages = new Queue<string> (mesCount);

		PhotonNetwork.logLevel = PhotonLogLevel.Full;
		PhotonNetwork.ConnectUsingSettings ("0.2");
		StartCoroutine ("UpdateConnectionString");

	}	
	/*
	 * This will get Photons connection status
	 */
	IEnumerator UpdateConnectionString () 
	{
		while (true) 
		{
			connectionText.text = PhotonNetwork.connectionStateDetailed.ToString ();
			yield return null;
		}
	}

	/*
	 * This method will display the join room menu to the users
	 * 
	 */
	void OnJoinedLobby()
	{
		ScoreBoard.SetActive (false);
		serverWindow.SetActive (true);
		StopCoroutine ("UpdateConnectionString");
		connectionText.text = "";
		username.text = "";
		roomName.text = "";
	}

	/*
	 * This will update the room list display. This will show open games.
	 * 
	 */
	void OnReceivedRoomListUpdate ()
	{
		roomList.text = "";
		foreach (RoomInfo room in PhotonNetwork.GetRoomList ())
			
		{
			roomList.text += room.name + "\n";
		}
	}

	/*
	 * This method will either create or join the room that the user has entered. 
	 * 
	 */
	public void JoinRoom()
	{
		RoomOptions ro = new RoomOptions (){isVisible = true, maxPlayers = 10};
		//PhotonNetwork.player.name = username.text;
		PhotonNetwork.JoinOrCreateRoom (roomName.text, ro, TypedLobby.Default);
	}
	/*
	 * This method will deactivate the users room menu, and display the name entery menu.
	 * 
	 */
	void OnJoinedRoom()
	{
		serverWindow.SetActive (false);
		nameWindow.SetActive (true);
	}

	/*
	 * Will validate the username input.
	 * 
	 */
	public void checkPlayerName()
	{
		string enteredName = username.text;
		bool nameAvailable = true;
		foreach(PhotonPlayer player in PhotonNetwork.playerList)
		{
			Debug.Log(player.name);
			if(player.name.Equals (enteredName))
			{
				Debug.Log(player.name);
				nameAvailable = false;
			}
		}
		if (nameAvailable == true) 
		{
			PhotonNetwork.player.name = enteredName;
			nameWindow.SetActive (false);
			healthPanel.SetActive (true);
			MessageBox.SetActive (true);
			StartSpawnProcess (0f, 0f, 0f);
		} else {
			username.text = "USERNAME ALREADY USED!";
		}
	}

	/*
	 * This method will activate the players camera, and start the spawning process.
	 * 
	 */
	void StartSpawnProcess(float respawnTime, float kills, float deaths)
	{
		playerCam.enabled = true;
		roomCam.enabled = false;

		StartCoroutine (SpawnPlayer(respawnTime, kills, deaths));
	}

	/*
	 * This Coroutine will spawn a player across the network. It will also intilise some of 
	 * the players stats. It will also attach the camera and relevant scripts to the players 
	 * ghost.
	 * 
	 */
	IEnumerator SpawnPlayer(float respawnTime, float kills, float deaths)
	{
		yield return new WaitForSeconds (respawnTime);

		int index = Random.Range (0, spawnPoints.Length);

		player = PhotonNetwork.Instantiate ("PlayerGhost", spawnPoints[index].position, spawnPoints[index].rotation, 0);
		player.GetComponent<PlayerNetworkMovingScript> ().RespawnPlayer += StartSpawnProcess;
		player.GetComponent<PlayerNetworkMovingScript> ().SendNetworkMessage += AddMessage;
		//player.GetComponent<PlayerScript> ().UpdatePlayerHealthBar += updateHealthBar;
		player.GetComponent<PlayerScript> ().EndCurrentGame += displayScoreBoard;

		AddMessage ("Spawned player: " + PhotonNetwork.player.name);

		player.GetComponent<PlayerScript>().kills = kills;
		player.GetComponent<PlayerScript>().deaths = deaths;
		player.GetComponent<PlayerScript>().playerID = PhotonNetwork.player.ID;
		float playerHealth = player.GetComponent<PlayerScript> ().health;
		healthBox.text = "Health: " + playerHealth;
		// allows to walk through walls!
		player.layer = 8;
		GameObject camera = GameObject.FindWithTag ("MainCamera");
		if (camera != null) 
		{
			PlayerCameraController followScript = camera.GetComponent("PlayerCameraController") as PlayerCameraController;
			if (followScript != null)
			{
				followScript.target = player;
			}
		}
	}
	void updateHealthBar(float health)
	{
		healthBox.text = "Health: " + health;
	}

	/*
	 * This method will call the RPC to broadcast the message. It will send it too all players.
	 * 
	 */
	void AddMessage(string message)
	{
		photonView.RPC ("AddMessage_RPC", PhotonTargets.All, message);
	}
	/*
	 * THis RPC will add a message to the message queue. If it is 
	 * 
	 */
	[RPC] 
	void AddMessage_RPC(string message)
	{
		messages.Enqueue (message);
		if (messages.Count > mesCount)
		{
			messages.Dequeue ();
		}
		messageWindow.text = "";
		foreach (string mes in messages) 
		{
			messageWindow.text += mes + "\n";
		}
	}
	/*
	 * This method will call the RPC to display the ScoreBoard to all players.
	 * 
	 */
	void displayScoreBoard(string WinnersMessage)
	{
		photonView.RPC ("displayGameResult_RPC", PhotonTargets.All, WinnersMessage);
	}
	/*
	 * This RPC will display the final result when the game has ended. 
	 *
	 * I have left code for actual scoreboard, not complete!
	 *
	 */
	[RPC]
	void displayGameResult_RPC(string WinnersMessage)
	{
		ScoreBoard.SetActive (true);
		ScoreBox.text = "";
		ScoreBox.text += WinnersMessage + "\n" + "\n";

		/*
		ScoreBox.text += "They killed " + lastDeathID + " to win!" + "\n" + "\n";

		float kills = player.GetComponent<PlayerScript>().kills;
		float deaths = player.GetComponent<PlayerScript>().deaths;
		ScoreBox.text += "Your name: " + PhotonNetwork.player.name + "\n";
		ScoreBox.text += PhotonNetwork.player.name + "'s score: " + kills + "-" + deaths + " (K/D)" + "\n";

		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		string curPlayerName = "";
		float curPlayerKills = 0f;
		float curPlayerDeaths = 0f;

		foreach (GameObject player in players) 
		{
			curPlayerName = PhotonNetwork.player.name;
			curPlayerKills = player.GetComponent<PlayerScript>().kills;
			curPlayerDeaths = player.GetComponent<PlayerScript>().deaths;
			ScoreBox.text += curPlayerName + ": " + curPlayerKills + "-" + curPlayerDeaths + " (K/D)" + "\n";
		}*/

	}

	/*
	 * This method will display the game instructions to the player. 
	 * 
	 */
	public void displayInstructions()
	{
		serverWindow.SetActive (false);
		InstructionPanel.SetActive (true);
	}
	/*
	 * This method will return the player back to the main room menu from instruction screen.
	 * 
	 */
	public void returnFromInstructions()
	{
		InstructionPanel.SetActive (false);
		serverWindow.SetActive (true);
	}
	/*
	 * This method will let the player leave the room, and reactivate the room menu for the player.
	 * 
	 */
	public void endGame()
	{
		PhotonNetwork.LeaveRoom();
		messageWindow.text = "";
		MessageBox.SetActive (false);
		healthPanel.SetActive (false);
		OnJoinedLobby ();
	}
	/*
	 * This method will exit the game. 
	 *
	 */
	public void exitApplication()
	{
		Application.Quit();
	}
}