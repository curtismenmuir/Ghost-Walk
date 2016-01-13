using UnityEngine;
using System.Collections;

public class Shooting : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	/*
	 * Will create bullet over the network when player hits the fire key.
	 * 
	 */
	void Update ()
	{
		if (Input.GetButtonDown ("Fire1")) {
			float h = Input.GetAxis ("Horizontal");
			float v = Input.GetAxis ("Vertical");
			if (v == 0 && h == 0) 
			{
			}else{
				GameObject var = PhotonNetwork.Instantiate ("Blast1", transform.position, transform.rotation, 0);
				GetComponent<AudioSource>().Play();
				var.layer = 10;
				Destroy (var, 3);
				var.GetComponent<BulletMovement> ().horizontal = h;
				var.GetComponent<BulletMovement> ().verticle = v;
				var.GetComponent<BulletMovement> ().speed = 15f;
			}
		}
	}
}