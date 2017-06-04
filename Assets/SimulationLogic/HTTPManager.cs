using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTTPManager : MonoBehaviour {
	string serverURL = "http://30b26fbb.ngrok.io/fileupload";

	public void SendPhotoToServer(byte[] screenShot, string fileName, string coordinates) {
		var coroutine = UploadPNG(screenShot, fileName, coordinates);
		StartCoroutine(coroutine);
	}

	IEnumerator UploadPNG(byte[] screenshot, string fileName, string coordinates) {

		WWWForm serverForm = new WWWForm();
		serverForm.AddBinaryData("fileUpload", screenshot, fileName, "image/png");
		serverForm.AddField("coordinates", coordinates);

		WWW w = new WWW(serverURL, serverForm);
		yield return w;
		if (!PropertyName.IsNullOrEmpty(w.error)) {
			Debug.Log(w.error);
		} else {
			Debug.Log("FINISHED UPLOADING");
		}
	}
}
