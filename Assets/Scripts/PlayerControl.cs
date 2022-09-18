using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour
{
    public TextMeshProUGUI speedIndicator;
    public GameObject rocketPrefab;
    public Transform fireFrom;

    float rollSpeed = 60.0f;
    float pitchSpeed = 40.0f;
    float strafeSpeed = 20.0f;
    float forwardAccel = 70.0f;
    float speedNow = 0.0f;
    float maxNegativeSpeed = -20.0f;
    float maxForwardSpeed = 60.0f;

    void Update()
    {
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
