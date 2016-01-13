using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour 
{
	/*
	 * Used to end game and display scoreboard over network.
	 *
	 */
	public delegate void EndGame(string WinnersMessage);
	public event EndGame EndCurrentGame;

	//public delegate void UpdateHealthBar(float health);
	//public event UpdateHealthBar UpdatePlayerHealthBar;

	public int playerID;
	public float health = 100f;
	public float speed = 8f;
	public float damage = 20f;
	public float kills;
	public float deaths;

	void Start () 
	{
	}
	/*
	 * Moves the players position depending on thier input. 
	 * 
	 */
	void FixedUpdate ()
	{
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		GetComponent<Rigidbody2D>().velocity = new Vector2 (Mathf.Lerp (0, h * speed, 0.8f), Mathf.Lerp (0, v * speed, 0.8f));
	}
	/*
	 * This method is called when anotehr game object collides with the player.If it is a wall, 
	 * then it will do nothing, else it will be a bullet.It will then reduce the bullets damage 
	 * and check if player has been killed. 
	 * 
	 */
	void OnTriggerEnter2D (Collider2D myTrigger)
	{
		Debug.Log("Collision Detected");
		string layerName = LayerMask.LayerToName (myTrigger.gameObject.layer);
		Debug.Log ("Players Health: " + health);
		Debug.Log ("LayerName: " + layerName);
		if (layerName == "Wall") {

		} else {
			float damage = myTrigger.GetComponent<BulletMovement>().bulletDamage;
			health = health - damage;
			// CODE HERE TO OBDATE PLAYER HEALTH BAR!!!!
			// UpdatePlayerHealthBar(health);
			if (health == 0f) {
				Debug.Log ("Player Killed");
				string enemy = myTrigger.GetComponent<BulletMovement> ().playerName;
				int enemyID = myTrigger.GetComponent<BulletMovement> ().playerID;

				IncrementEnemyKillScore (enemyID);

				transform.GetComponent<PhotonView> ().RPC ("killPlayer", PhotonTargets.All, enemy);
			}
		}
	}
	/*
	 * This method will increment a players kill score when they have killed another player. If the player
	 * gets 5 kills, they are crownd the winner.
	 * 
	 */
	void IncrementPlayerKillScore()
	{
		kills = kills + 1;
		if (kills == 5f) 
		{
			if (EndCurrentGame != null) 
			{
				string WinnersMessage = PhotonNetwork.player.name + " is the winner!";
				EndCurrentGame(WinnersMessage);
			}
		}
		transform.GetComponent<PhotonView>().RPC ("displayPlayerScore", PhotonTargets.All, kills, deaths);
	}

	/*
	 * this method will find the player that shot the blast that killed them, and will call a method to add one to their score.
	 * 
	 */
	public void IncrementEnemyKillScore(int enemyID)
	{
		//int temp = gameObject.GetComponent<PlayerScript> ().playerID;
		//Debug.Log ("playerID2 :" + temp);
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject player in players) {
			if(player.GetComponent<PhotonView>().ownerId == enemyID)
			{
				player.GetComponent<PlayerScript>().IncrementPlayerKillScore();
			}
		}
	}
}