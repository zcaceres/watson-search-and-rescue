using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWaypointGrid : MonoBehaviour {
	private DroneManager droneManager;
	public GameObject waypointPrefab;
	int WAYPOINT_HEIGHT_Y_VALUE = 100;

	void Awake() {
		droneManager = GameObject.Find("DroneManager").GetComponent<DroneManager>();
		GenerateGrid();
		SpawnDrones();
	}

	void GenerateGrid () {
		for (var z = 0; z <= 1000; z+= 100) {
			GenerateRow(z);
		}
		Debug.Log("Waypoint Grid Generated");
	}

	void SpawnDrones() {
		Debug.Log("Spawning Drones");
		droneManager.EnableDrones();
	}

	void GenerateRow (int zCoord) {
		for (var x = 0; x <= 1000; x += 100) {
			PlaceWaypoint(x, zCoord);
		}
	}

	void PlaceWaypoint(int xCoord, int zCoord) {
		Vector3 gridPosition = new Vector3 (xCoord, WAYPOINT_HEIGHT_Y_VALUE, zCoord);
		Instantiate(waypointPrefab, gridPosition, Quaternion.identity, this.transform);
	}

}
