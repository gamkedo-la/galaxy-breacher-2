using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl instance;
    public TextMeshProUGUI speedIndicator;
    public GameObject rocketPrefab;
    public Transform fireFrom;

    public AudioSource engineLoopWeak;
    public AudioSource engineLoopStrong;

    float rollSpeed = 60.0f;
    float pitchSpeed = 40.0f;
    float strafeSpeed = 20.0f;
    float forwardAccel = 70.0f;
    float speedNow = 0.0f;
    float maxNegativeSpeed = -20.0f;
    float maxForwardSpeed = 60.0f;

    void Awake() 
    {
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

    void Update()
    {
        RefreshEngineVolume();

        speedNow += Input.GetAxis("Throttle") * forwardAccel * Time.deltaTime;
        speedNow = Mathf.Clamp(speedNow, maxNegativeSpeed, maxForwardSpeed);
        transform.Rotate(Vector3.forward, Input.GetAxis("Roll") * -rollSpeed * Time.deltaTime);
        transform.Rotate(Vector3.right, Input.GetAxis("Pitch") * pitchSpeed * Time.deltaTime);
        transform.position += transform.right * Input.GetAxis("Horizontal") * strafeSpeed * Time.deltaTime;
        transform.position += transform.up* Input.GetAxis("Vertical") * strafeSpeed * Time.deltaTime;
        transform.position += transform.forward * speedNow * Time.deltaTime;
        speedIndicator.text = "Speed: "+Mathf.Round(speedNow * 10.0f);

        if(Input.GetButtonDown("Fire1")) {
            GameObject shotGO = GameObject.Instantiate(rocketPrefab, fireFrom.position, transform.rotation);
        }
    }
}
