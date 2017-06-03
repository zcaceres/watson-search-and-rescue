using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTTPManager : MonoBehaviour {
	string serverURL = "http://d7476323.ngrok.io/fileupload";

	public void SendPhotoToServer(byte[] screenShot, string fileName, string coordinates) {
		var coroutine = UploadPNG(screenShot, fileName, coordinates);
		StartCoroutine(coroutine);
	}

	IEnumerator UploadPNG(byte[] screenshot, string fileName, string coordinates) {
		// byte[] bytes = screenshot;
		// public function AddField(fieldName: string, value: string,
		// e: Encoding = System.Text.Encoding.UTF8): void;

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
