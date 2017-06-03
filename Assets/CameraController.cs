using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	Camera myCamera;
	bool isWaitingForCamera;
	int imageIndex = 0;
	// Use this for initialization
	void OnActive() {
		myCamera = GetComponent<Camera>();
	}

	void Start () {

	}

	IEnumerator TakePhotos(int intervalToPhoto) {
		Debug.Log("TAKING A PHOTO!");
		yield return new WaitForSeconds(intervalToPhoto);
		Application.CaptureScreenshot("./Assets/images/drone-id-image-" + imageIndex + ".png");
		imageIndex++;
		yield return new WaitForSeconds(1);
		isWaitingForCamera = false;
	}


	// Update is called once per frame
	void Update () {
		if (!isWaitingForCamera) {
			isWaitingForCamera = true;
			StartCoroutine("TakePhotos", 5);
		}
	}
}
