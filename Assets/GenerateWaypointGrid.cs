using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWaypointGrid : MonoBehaviour {
	private DroneManager droneManager;
	public GameObject rowPrefab;
	public GameObject waypointPrefab;
	int WAYPOINT_HEIGHT_Y_VALUE = 100;

	void Awake() {
		droneManager = GameObject.Find("DroneManager").GetComponent<DroneManager>();
		GenerateGrid();
		SpawnDrones();
	}

	void GenerateGrid () {
		int rowCounter = 0;
		for (var z = 0; z <= 1000; z+= 100) {
			GenerateRow(z, rowCounter);
			rowCounter++;
		}
		Debug.Log("Waypoint Grid Generated");
	}

	void SpawnDrones() {
		Debug.Log("Spawning Drones");
		droneManager.EnableDrones();
	}

	void GenerateRow (int zCoord, int rowCounter) {
		var row = CreateRowPrefab(rowCounter);
		for (var x = 0; x <= 1000; x += 100) {
			PlaceWaypoint(x, zCoord, row);
		}
	}

	GameObject CreateRowPrefab(int rowCounter) {
		var row = Instantiate(rowPrefab, this.transform);
		row.name = "row-" + rowCounter;
		return row;
	}

	void PlaceWaypoint(int xCoord, int zCoord, GameObject row) {
		Vector3 gridPosition = new Vector3 (xCoord, WAYPOINT_HEIGHT_Y_VALUE, zCoord);
		Instantiate(waypointPrefab, gridPosition, Quaternion.identity, row.transform);
	}

}
