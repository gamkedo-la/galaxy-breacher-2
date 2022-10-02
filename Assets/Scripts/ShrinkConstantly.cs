using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkConstantly : MonoBehaviour
{
    float scaleAfterEachFixedTick = 0.98f;

    private BarometerDetectable baroDetect;

	void Start() {
        baroDetect = GetComponent<BarometerDetectable>();
	}

	void FixedUpdate()
    {
        transform.localScale *= scaleAfterEachFixedTick;
        if (baroDetect) baroDetect.size *= scaleAfterEachFixedTick;
    }
}
