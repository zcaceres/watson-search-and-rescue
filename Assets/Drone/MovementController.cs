﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		var newPosition = transform.position;
		newPosition.x += 1.0f;
		transform.position = newPosition;
	}
}
