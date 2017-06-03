using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour {
	private GameObject waypointManager;

	void Awake() {
		waypointManager = GameObject.Find("WaypointManager");
	}

	public void EnableDrones() {
		var rowCounter = 0;
		foreach (Transform child in transform) {
			var waypointRow = GetWaypointRow(rowCounter);
			SetDroneWaypointRow(waypointRow, child);
			Debug.Log("Enabling Drone: " + child);
			child.gameObject.SetActive(true);
			rowCounter++;
		}
	}

	Transform[] GetWaypointRow(int rowCounter) {
		Transform[] waypoints = new Transform[11];
		var currentRow = waypointManager.transform.Find("row-" + rowCounter);
		var counter = 0;
		foreach (Transform waypoint in currentRow) {
			waypoints[counter] = waypoint;
			counter++;
		}
		Debug.Log("Current row assigning to drone " + currentRow);
		return waypoints;
		// find row prefab in Waypoint Manager
		// assign every child as queue inside drone (will need property on drone)
		//
	}

	void SetDroneWaypointRow(Transform[] waypoints, Transform drone) {
		drone.GetComponent<MovementController>().SetWaypoints(waypoints);
 		Debug.Log("Giving drone waypoints " + waypoints + " " + drone);
	}

}
