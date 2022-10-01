using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarometerImageDrawer : MonoBehaviour {
	public float updateInterval = 0.1f;
	public float signalGain = 1000f;

	private Texture2D image;
	private int res = 64;
	private int line = 0;

	private float nextDrawTime = 0f;

	void Start() {
		image = new Texture2D(res, res, TextureFormat.ARGB32, false);
		GetComponent<Renderer>().material.SetTexture("_BaseMap", image);

		for (int x = 0; x < res; x++) {
			for (int y = 0; y < res; y++) {
				image.SetPixel(x, y, Color.black);
			}
		}
	}

	void Update() {
		if (Time.time > nextDrawTime) {
			DrawMeter();

			nextDrawTime = Time.time + updateInterval;
		}

	}

	private void DrawMeter() {

		int nextLine = (line + 1) % res;
		for (int i = 0; i < res; i++) {
			image.SetPixel(i, line, Color.black);
			image.SetPixel(i, nextLine, Color.green);
		}

		foreach (BarometerDetectable target in BarometerDetectable.masterList) {
			float dist = 1f - Mathf.Clamp(Vector3.Distance(target.transform.position, transform.position) / signalGain, 0f, 1f);
			if (dist <= 0f) continue;

			Vector3 targetDir = target.transform.position - transform.position;
			float angle = Vector3.SignedAngle(targetDir, transform.forward, transform.up) + 180f;
			int x = (int)(angle/360f * res);

			image.SetPixel(x, line, Color.HSVToRGB(target.colorHueValue, 1f, dist) + image.GetPixel(x, line));
		}

		image.Apply();

		line++;
	}
}
