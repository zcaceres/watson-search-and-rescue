﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
	private int waypointCounter = 0;
	public Transform currentWaypoint;
	float DRONE_FLIGHT_SPEED = 0.5f;
	[SerializeField] private Transform[] myWaypoints;

	// Update is called once per frame
	void Update () {
		MoveDrone();
		HasReachedWaypoint();
	}

	public void SetWaypoints(Transform[] waypoints) {
		myWaypoints = waypoints;
		UpdateWaypoint();
	}

	void MoveDrone() {
		var targetPosition = currentWaypoint.position;
		this.transform.position = Vector3.Slerp(this.transform.position,
			targetPosition, DRONE_FLIGHT_SPEED*Time.deltaTime);
	}

	// Called from CameraController after Photo to advance drone
	public void AdvanceDrone() {
		UpdateWaypoint();
	}

	void HasReachedWaypoint() {

	}

	void UpdateWaypoint() {
		if (waypointCounter < myWaypoints.Length) {
			currentWaypoint = myWaypoints[waypointCounter++];
		} else {
			Debug.Log("At end of route");
		}
	}

}
