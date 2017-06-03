using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	Camera myCamera;
	bool isWaitingForCamera;
	int imageIndex = 0;
	public int droneId;
	private DroneManager droneManager;
	private int INTERVAL_TO_TAKE_PHOTO = 1;

	void Awake() {
		droneManager = GameObject.Find("DroneManager").GetComponent<DroneManager>();
		myCamera = transform.Find("DroneCam").GetComponent<Camera>();
	}

	public void StartTakingPhotos() {
		StartCoroutine("TakePhotos", INTERVAL_TO_TAKE_PHOTO);
	}

	IEnumerator TakePhotos(int intervalToPhoto) {
		TurnOnCamera();
		// UPDATE UI
		yield return new WaitForSeconds(intervalToPhoto);
		Debug.Log("Capturing screenshot on drone " + droneId);
		Application.CaptureScreenshot("./Assets/images/drone-" + droneId + "-image-" + imageIndex + ".png");
		imageIndex++;
		yield return new WaitForSeconds(1);
		TurnOffCamera();
		NotifyDroneManagerThatDroneReady();
		isWaitingForCamera = false;
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
