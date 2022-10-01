using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarometerDetectable : MonoBehaviour {
	public static List<BarometerDetectable> masterList = new List<BarometerDetectable>();

	[Range(0f, 1f)]
	public float colorHueValue = 0f;

	void OnEnable() {
		masterList.Add(this);
	}

	private void OnDisable() {
		masterList.Remove(this);
	}

	void OnDestroy() {
		masterList.Remove(this);
	}
}
