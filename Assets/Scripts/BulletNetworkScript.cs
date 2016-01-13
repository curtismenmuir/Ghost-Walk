using UnityEngine;
using System.Collections;

public class BulletNetworkScript : Photon.MonoBehaviour {

	// Will activate BulletMovement script.
	void Start () {
		if (photonView.isMine)
		{
			GetComponent<BulletMovement>().enabled = true;
		}
	}

	void Update () {
	
	}
}
