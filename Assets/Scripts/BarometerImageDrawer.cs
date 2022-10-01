using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarometerImageDrawer : MonoBehaviour {
	public float updateInterval = 0.1f;
	public float signalGain = 1000f;

	private Texture2D image;
	[SerializeField]private int hRes = 128;
	[SerializeField] private int vRes = 64;
	private int line = 0;

	private float nextDrawTime = 0f;

	void Start() {
		image = new Texture2D(hRes, vRes, TextureFormat.ARGB32, false);
		GetComponent<Renderer>().material.SetTexture("_BaseMap", image);

		for (int x = 0; x < hRes; x++) {
			for (int y = 0; y < hRes; y++) {
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

		int nextLine = (line + 1) % vRes;
		int lastLine = (line + hRes - 1) % vRes;

		for (int i = 0; i < hRes; i++) {
			image.SetPixel(i, line, Color.black);
			image.SetPixel(i, nextLine, Color.green);
		}

		foreach (BarometerDetectable target in BarometerDetectable.masterList) {
			float dist = 1f - Mathf.Clamp(Vector3.Distance(target.transform.position, transform.position) / signalGain, 0f, 1f);
			if (dist <= 0f) continue;

			Vector3 targetDir = target.transform.position - transform.position;
			float angle = Vector3.SignedAngle(targetDir, transform.forward, transform.up) + 180f;
			int x = (int)(angle/360f * hRes);
			int x1 = (x + 1) % hRes;
			int x2 = (x + hRes - 1) % hRes;

			Color drawColor = Color.HSVToRGB(target.colorHueValue, 1f, dist) + image.GetPixel(x, line);

			image.SetPixel(x, line, drawColor);
			image.SetPixel(x1, line, drawColor);
			image.SetPixel(x2, line, drawColor);
			image.SetPixel(x, lastLine, drawColor);
			image.SetPixel(x, nextLine, drawColor);
		}

		image.Apply();

		line++;
	}
}
