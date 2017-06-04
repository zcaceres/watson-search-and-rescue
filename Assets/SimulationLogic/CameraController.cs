using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CameraController : MonoBehaviour {
	Camera myCamera;
	bool isWaitingForCamera;
	int imageIndex = 0;
	public int droneId;
	private DroneManager droneManager;
	private int INTERVAL_TO_TAKE_PHOTO = 1;
	private int photosTaken =  0;
	private MovementController movementController;
	private HTTPManager httpManager;

	void Awake() {
		droneManager = GameObject.Find("DroneManager").GetComponent<DroneManager>();
		myCamera = transform.Find("DroneCam").GetComponent<Camera>();
		movementController = GetComponent<MovementController>();
		httpManager = GameObject.Find("HTTPManager").GetComponent<HTTPManager>();
	}

	public void StartTakingPhotos() {
		StartCoroutine("TakePhotos", INTERVAL_TO_TAKE_PHOTO);
	}

	IEnumerator TakePhotos(int intervalToPhoto) {
		TurnOnCamera();
		// UPDATE UI
		yield return new WaitForSeconds(intervalToPhoto);
		var fileName = "./Assets/images/drone-" + droneId +
			"-image-" + imageIndex + ".png";
		var coordinates = (int)transform.position.x + "-" + (int)transform.position.z;

		Application.CaptureScreenshot(fileName);
		imageIndex++;
		yield return new WaitForSeconds(1);
		var newFile = RetrieveScreenshotFile(fileName);
		TurnOffCamera();
		NotifyDroneManagerThatDroneReady();
		isWaitingForCamera = false;
		httpManager.SendPhotoToServer(newFile, fileName, coordinates);
		if (photosTaken <= 10) {
			movementController.AdvanceDrone();
			photosTaken++;
			Debug.Log("PHOTOS TAKEN FOR DRONE " + droneId + " are " + photosTaken);
		} else {
			Debug.Log("ALL PHOTOS TAKEN");
		}
	}

	byte[] RetrieveScreenshotFile(string fileName) {
		byte[] bytes = File.ReadAllBytes(fileName);
		return bytes;
	}

	void TurnOffCamera() {
		Debug.Log("TURNING OFF CAMERA");
		myCamera.enabled = false;
	}

	void TurnOnCamera() {
		Debug.Log("TURNING ON CAMERA");
		myCamera.enabled = true;
	}

	void NotifyDroneManagerThatDroneReady() {
		Debug.Log("NOTIFYING DRONE MANAGER THAT IM READY");
		droneManager.NotifiedThatDroneReady();
	}

	void Update () {
		if (!isWaitingForCamera) {
			isWaitingForCamera = true;
			// StartCoroutine("TakePhotos", 5);
		}
	}
}
