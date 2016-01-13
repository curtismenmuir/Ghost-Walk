using UnityEngine;
using System.Collections;

public class BulletMovement : MonoBehaviour {
	public float horizontal;
	public float verticle;
	public float speed;
	public string playerName;
	public int playerID;
	public float bulletDamage;

	/*
	 * This method will with add the velocity to the bullet. It will also store all the information the bullet needs it its 
	 * model.
	 * 
	 */
	void Start () 
	{
		GetComponent<Rigidbody2D>().velocity = new Vector2 (Mathf.Lerp (0, horizontal * speed, 0.8f), Mathf.Lerp (0, verticle * speed, 0.8f));
		playerName = PhotonNetwork.player.name;
		playerID = PhotonNetwork.player.ID;
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject player in players) {
			if(player.GetComponent<PhotonView>().ownerId == playerID)
			{
				bulletDamage = player.GetComponent<PlayerScript>().damage;
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	/*
	 * This method will be called when the bullet collides with another game object. It destroy the blast on collision.
	 * 
	 */
	void OnTriggerEnter2D (Collider2D myTrigger)
	{
		Debug.Log("Collision Detected");
		/*if (myTrigger.gameObject.name == "PlayerGhost")
		Debug.Log("Collision with player");
		int enemyID = myTrigger.GetComponent<PlayerScript> ().playerID;
		float enemyHealth = myTrigger.GetComponent<PlayerScript> ().health - 50;
		if (enemyHealth <= 50) {
			myTrigger.GetComponent<PlayerScript>().IncrementEnemyKillScore(PhotonNetwork.player.ID, enemyID);
		} else {
			myTrigger.GetComponent<PlayerScript> ().health = enemyHealth;
		}*/
		PhotonNetwork.Destroy(gameObject);
	}
}
