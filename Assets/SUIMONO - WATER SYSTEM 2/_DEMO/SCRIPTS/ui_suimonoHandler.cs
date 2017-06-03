using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class ui_suimonoHandler : MonoBehaviour {


	public float uiScale = 1.0f;

	private Transform lightObject;
	private Suimono.Core.SuimonoObject suimonoObject;
	private CanvasScaler uiCanvasScale;
	private Slider sliderTOD;
	private Slider sliderBeaufort;


	void Start () {

		//get main object
		lightObject = GameObject.Find("Directional Light").GetComponent<Transform>();
		suimonoObject = GameObject.Find("SUIMONO_Surface").GetComponent<Suimono.Core.SuimonoObject>();
		uiCanvasScale = this.transform.GetComponent<CanvasScaler>();

		//find UI objects
		sliderTOD = GameObject.Find("Slider_TOD").GetComponent<Slider>();
		sliderBeaufort = GameObject.Find("Slider_Beaufort").GetComponent<Slider>();

	}






	void LateUpdate(){

		//CANVAS SCALE
		if (uiCanvasScale != null) uiCanvasScale.scaleFactor = uiScale;

		//########################
		// SET TIME OF DAY
		//########################
		if (lightObject != null && sliderTOD != null) lightObject.localEulerAngles = new Vector3(
			Mathf.Lerp(-15.0f,90.0f,sliderTOD.value),
			lightObject.localEulerAngles.y,
			lightObject.localEulerAngles.z
			);

		//###########################
		// SET SUIMONO SETTINGS
		//###########################
		if (suimonoObject != null) suimonoObject.beaufortScale = sliderBeaufort.value;

	}


}