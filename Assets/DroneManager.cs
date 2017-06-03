using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour {
	private GameObject waypointManager;
	private int currentDroneActivated = 0;
	[SerializeField] private GameObject[] allDrones;
	private int droneCounter;

	void Awake() {
		waypointManager = GameObject.Find("WaypointManager");
		GetAllDrones();
		droneCounter = allDrones.Length;
	}

	void GetAllDrones() {
		GameObject[] drones = new GameObject[11];
		int counter = 0;
		foreach (Transform t in transform) {
			drones[counter++] = t.gameObject;
		}
		allDrones = drones;
	}

	public void EnableDrones() {
		var rowCounter = 0;
		foreach (Transform child in transform) {
			var waypointRow = GetWaypointRow(rowCounter);
			SetDroneWaypointRow(waypointRow, child);
			child.gameObject.SetActive(true);
			Debug.Log("Enabling Drone: " + child); 
			rowCounter++;
		}
		TakePhotoFromNextDrone();
	}

	Transform[] GetWaypointRow(int rowCounter) {
		Transform[] waypoints = new Transform[11];
		var currentRow = waypointManager.transform.Find("row-" + rowCounter);
		var counter = 0;
		foreach (Transform waypoint in currentRow) {
			waypoints[counter] = waypoint;
			counter++;
		}
		return waypoints;
	}

	void SetDroneWaypointRow(Transform[] waypoints, Transform drone) {
		drone.GetComponent<MovementController>().SetWaypoints(waypoints);
	}

	// invoked by current activated drone
	public void NotifiedThatDroneReady() {
		TakePhotoFromNextDrone();
	}

	// tells next drone to take a photo
	void TakePhotoFromNextDrone() {
		if (droneCounter >= allDrones.Length) {
			droneCounter = 0;
		}
		var droneCam = allDrones[droneCounter++].GetComponent<CameraController>();
		droneCam.StartTakingPhotos();
	}

}
