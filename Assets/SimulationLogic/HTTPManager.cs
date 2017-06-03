using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTTPManager : MonoBehaviour {
	string serverURL = "http://d7476323.ngrok.io/fileupload";

	public void SendPhotoToServer(byte[] screenShot, string fileName) {
		var coroutine = UploadPNG(screenShot, fileName);
		StartCoroutine(coroutine);
	}

	IEnumerator UploadPNG(byte[] screenshot, string fileName) {
		// byte[] bytes = screenshot;

		WWWForm serverForm = new WWWForm();
		serverForm.AddBinaryData("fileUpload", screenshot, fileName, "image/png");
		// Add field for X,Z coords here

		WWW w = new WWW(serverURL, serverForm);
		yield return w;
		if (!PropertyName.IsNullOrEmpty(w.error)) {
			Debug.Log(w.error);
		} else {
			Debug.Log("FINISHED UPLOADING");
		}
	}
}
