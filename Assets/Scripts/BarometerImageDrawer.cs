using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarometerImageDrawer : MonoBehaviour {
	public GameObject referanceObject;
	public float updateInterval = 0.1f;
	public float signalGain = 1000f;

	private Texture2D image;
	[SerializeField]private int hRes = 128;
	[SerializeField] private int vRes = 64;
	private int line = 0;
	private int refLine1 = 0;
	private int refLine2 = 0;
	private int refLine3 = 0;

	private float nextDrawTime = 0f;

	void Start() {
		image = new Texture2D(hRes, vRes, TextureFormat.ARGB32, false);
		GetComponent<Renderer>().material.SetTexture("_BaseMap", image);

		for (int x = 0; x < hRes; x++) {
			for (int y = 0; y < hRes; y++) {
				image.SetPixel(x, y, Color.black);
			}
		}

		refLine1 = hRes / 2;
		refLine2 = hRes / 4;
		refLine3 = refLine2 * 3;

		if (!referanceObject) referanceObject = gameObject;
	}

	void Update() {
		if (Time.time > nextDrawTime) {
			DrawMeter();

			nextDrawTime = Time.time + updateInterval;
		}

	}

	private void DrawMeter() {

		int nextLine = (line + 1) % vRes;
		int lastLine = (line + vRes - 1) % vRes;

		for (int i = 0; i < hRes; i++) {
			image.SetPixel(i, line, Color.black);
			image.SetPixel(i, nextLine, Color.green);
		}

		Color refLineColor = Color.HSVToRGB(0.333f, 10.5f, 0.5f);
		image.SetPixel(refLine1, line, refLineColor);
		image.SetPixel(refLine2, line, refLineColor);
		image.SetPixel(refLine3, line, refLineColor);
		image.SetPixel(0, line, refLineColor);
		image.SetPixel(hRes - 1, line, refLineColor);

		foreach (BarometerDetectable target in BarometerDetectable.masterList) {
			float dist = 1f - Mathf.Clamp(Vector3.Distance(target.transform.position, referanceObject.transform.position) / signalGain, 0f, 1f);
			if (dist <= 0f) continue;

			Vector3 targetDir = target.transform.position - referanceObject.transform.position;
			float angle = Vector3.SignedAngle(targetDir, referanceObject.transform.forward, referanceObject.transform.up) + 180f;
			int x = (int)(angle/360f * hRes);

			Color drawColor = Color.HSVToRGB(target.colorHueValue, 1f, dist);

			int halfSize = Mathf.CeilToInt(target.size / 2f);
			int bottomHalf = Mathf.CeilToInt(-target.size / 2f);
			for (int i = bottomHalf; i < halfSize; i++) {
				int x1 = (x + hRes + i) % hRes;
				image.SetPixel(x1, line, drawColor + image.GetPixel(x1, line));
			}

			image.SetPixel(x, nextLine, drawColor);
		}

		image.Apply();

		line++;
	}
}
