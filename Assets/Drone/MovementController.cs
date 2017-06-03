using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
	public Transform currentWaypoint;
	float DRONE_FLIGHT_SPEED = 0.1f;

	// Update is called once per frame
	void Update () {
		MoveDrone();
	}

	void MoveDrone() {
		var targetPosition = currentWaypoint.position;
		this.transform.position = Vector3.Slerp(this.transform.position,
			targetPosition, DRONE_FLIGHT_SPEED*Time.deltaTime);
	}

	void CheckIfWaypointReached() {


	}

	void UpdateWaypoint() {

	}

}
