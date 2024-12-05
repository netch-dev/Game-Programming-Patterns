using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// - To return the objects to the pool all we have to do is disable them
public class DisableObjects : MonoBehaviour {
	private void OnTriggerEnter(Collider collider) {
		collider.gameObject.SetActive(false);
	}
}