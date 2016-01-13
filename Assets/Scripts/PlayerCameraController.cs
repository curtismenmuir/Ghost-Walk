using UnityEngine;
using System.Collections;

public class PlayerCameraController : MonoBehaviour {

	public GameObject target;
	private Vector3 offset;

	void Start () {
		offset = transform.position;
	}

	void LateUpdate () {
		if (target != null) {
			transform.position = target.transform.position + offset;
		}
	}
}
