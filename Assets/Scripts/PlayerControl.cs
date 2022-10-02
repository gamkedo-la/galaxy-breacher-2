using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour {
    public static PlayerControl instance;
    public TextMeshProUGUI speedIndicator;
    public GameObject rocketPrefab;
    public Transform fireFrom;

    public AudioSource engineLoopWeak;
    public AudioSource engineLoopStrong;

    float rollSpeed = 60.0f;
    float pitchSpeed = 40.0f;
    float strafeSpeed = 20.0f;

    float speedNow = 0.0f;
    float maxNegativeSpeed = -20.0f;
    float maxForwardSpeed = 60.0f;

    float forwardAccel = 9.0f; // affects Q/E manual adjustment
    float throttleKeptFromPrevFrame = 0.985f; // affects 1-4 presets
    float throttleTarget = 0.0f;

    void Awake() {
        instance = this; // singleton for AI to aim etc
        engineLoopStrong.volume = 0.0f;
    }

    void RefreshEngineVolume() {
        float enginePowerFade = Mathf.Abs(speedNow / maxForwardSpeed); // velocity as main component

        float strafeEngineEffect = 0.55f * (
            Mathf.Abs(Input.GetAxis("Horizontal")) +
            Mathf.Abs(Input.GetAxis("Vertical")));

        float forwardOrStrafeEngineBalance = 0.6f;
        enginePowerFade = forwardOrStrafeEngineBalance * enginePowerFade + (1.0f - forwardOrStrafeEngineBalance) * strafeEngineEffect;

        engineLoopWeak.volume = 1.0f - enginePowerFade;
        engineLoopStrong.volume = enginePowerFade;
    }

    void Update() {
        RefreshEngineVolume();

        if (Mathf.Abs(Input.GetAxis("Throttle")) > 0.15f) { // manual throttle overrides 1-4 presets
            speedNow += Input.GetAxis("Throttle") * forwardAccel * Time.deltaTime;
            throttleTarget = speedNow;
        }
        speedNow = Mathf.Clamp(speedNow, maxNegativeSpeed, maxForwardSpeed);
        transform.Rotate(Vector3.forward, Input.GetAxis("Roll") * -rollSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right, Input.GetAxis("Pitch") * pitchSpeed * Time.deltaTime);
        transform.position += transform.right * Input.GetAxis("Horizontal") * strafeSpeed * Time.deltaTime;
        transform.position += transform.up * Input.GetAxis("Vertical") * strafeSpeed * Time.deltaTime;
        transform.position += transform.forward * speedNow * Time.deltaTime;
        speedIndicator.text = "Speed: " + Mathf.Round(speedNow * 10.0f);

        if (Input.GetButtonDown("Fire1")) {
            GameObject shotGO = GameObject.Instantiate(rocketPrefab, fireFrom.position, transform.rotation);
        }

        if (Input.GetButtonDown("Engine-Off")) {
            throttleTarget = 0.0f;
        }
        if (Input.GetButtonDown("Engine-Low")) {
            throttleTarget = maxForwardSpeed * 0.3f;
        }
        if (Input.GetButtonDown("Engine-Mid")) {
            throttleTarget = maxForwardSpeed * 0.6f;
        }
        if (Input.GetButtonDown("Engine-Max")) {
            throttleTarget = maxForwardSpeed * 1.0f;
        }
    }

    void FixedUpdate() { // using % per frame would be unsafe in variable framerate Update
        speedNow = speedNow* throttleKeptFromPrevFrame + throttleTarget * (1.0f- throttleKeptFromPrevFrame);
    }
}