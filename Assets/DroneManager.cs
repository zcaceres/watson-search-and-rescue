using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour {

	public void EnableDrones() {
		foreach (Transform child in transform) {
			Debug.Log("Enabling Drone: " + child);
			child.gameObject.SetActive(true);
		}
	}
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}
}
