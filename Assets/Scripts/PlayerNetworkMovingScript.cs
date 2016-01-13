using UnityEngine;
using System.Collections;

public class PlayerNetworkMovingScript : Photon.MonoBehaviour 
{
	public delegate void Respawn(float time, float kills, float deaths);
	public event Respawn RespawnPlayer;

	public delegate void SendMessage(string MessageOverlay);
	public event SendMessage SendNetworkMessage;

	float h;
	float v;
	float speed;
	public int playerID;
	bool move;

	// Use this for initialization
	void Start ()
	{
		if (photonView.isMine) 
		{
			GetComponent<PlayerScript> ().enabled = true;
			GetComponent<Shooting> ().enabled = true;
			playerID = PhotonNetwork.player.ID;
		} 
		else 
		{
			StartCoroutine("UpdateData");
		}
	}
	/*
	 * Coroutine to update data.
	 * 
	 */
	IEnumerator UpdateData()
	{
		while (true) 
		{
			transform.GetComponent<Rigidbody2D>().velocity = new Vector2 (Mathf.Lerp (0, h * speed, 0.8f), Mathf.Lerp (0, v * speed, 0.8f));
			yield return null;
		}
	}
	/*
	 * Will send/recieve players position over network. 
	 * 
	 */
	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) 
		{
			h = transform.GetComponent<BulletMovement>().horizontal = h;
			v = transform.GetComponent<BulletMovement>().verticle = v;
			speed = transform.GetComponent<BulletMovement>().speed;
			stream.SendNext (h);
			stream.SendNext (v);
			stream.SendNext (speed);
		} 
		else 
		{
			h = (float)stream.ReceiveNext();
			v = (float)stream.ReceiveNext();
			speed = (float)stream.ReceiveNext();
		}
	}
	/*
	 * This method will destroy a player over the network. 
	 * 
	 */
	[RPC]
	public void killPlayer(string enemy)
	{
		if (photonView.isMine) 
		{
			if(SendNetworkMessage != null)
			{
				SendNetworkMessage(PhotonNetwork.player.name + " was killed by " + enemy);
			}
			if (RespawnPlayer != null) 
			{	float kills = transform.GetComponent<PlayerScript>().kills;
				float deaths = transform.GetComponent<PlayerScript>().deaths;
				deaths = deaths + 1f;
				Debug.Log ("Respawn deaths: " + deaths);
				RespawnPlayer (3f, kills, deaths);
			}
			Debug.Log("Network Player Killed");
			PhotonNetwork.Destroy (gameObject);
		}
	}
	/*
	 * This method will display a players score.
	 * 
	 */
	[RPC]
	public void displayPlayerScore(float kills, float deaths)
	{
		if (photonView.isMine) 
		{
			if(SendNetworkMessage != null)
			{
				SendNetworkMessage(PhotonNetwork.player.name + ": " + kills + "-" + deaths + " (Kills-Deaths)");
			}
		}
	}
}