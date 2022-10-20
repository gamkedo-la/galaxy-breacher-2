using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour {
    public static PlayerControl instance;
    public TextMeshProUGUI speedIndicator;
    public GameObject rocketPrefab;
    public Transform fireFrom;
    public float range;

    public AudioSource engineLoopWeak;
    public AudioSource engineLoopStrong;

    public Animator damageLighting;
    public AudioSource damageSound;

    public Camera fpsCamera;
    private Rigidbody rb;

    Color color;

    [SerializeField] List<GameObject> targets = new List<GameObject>();

    [SerializeField] float rollSpeed = 60.0f;
    [SerializeField] float pitchSpeed = 40.0f;
    [SerializeField] float strafeSpeed = 20.0f;

    float speedNow = 0.0f;
    float maxNegativeSpeed = -20.0f;
    float maxForwardSpeed = 60.0f;

    float forwardAccel = 9.0f; // affects Q/E manual adjustment
    float throttleKeptFromPrevFrame = 0.985f; // affects 1-4 presets
    float throttleTarget = 0.0f;
    void Awake() {
        instance = this; // singleton for AI to aim etc
        engineLoopStrong.volume = 0.0f;
        rb = GetComponent<Rigidbody>();
        color = Color.red;
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
        Vector3 moveVec = Vector3.zero;
        // remember: no  * Time.deltaTime here since the .velocity of rb already handles that
        moveVec += transform.right * Input.GetAxis("Horizontal") * strafeSpeed;
        moveVec += transform.up * Input.GetAxis("Vertical") * strafeSpeed;
        moveVec += transform.forward * speedNow;
        rb.velocity = moveVec;
        speedIndicator.text = "Speed: " + Mathf.Round(speedNow * 100.0f);

        if (Input.GetButtonDown("Fire1")) {

           Shoot();
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

        if (Input.GetKeyDown(KeyCode.T))
        {
            print("Take Damage Lights: Eventually move this to where player loses health");
            damageLighting.SetTrigger("Damage");
            damageSound.Play();
        }

        Debug.DrawRay(fpsCamera.transform.position, this.transform.forward * range, color,5f); ;

    }

    void FixedUpdate() { // using % per frame would be unsafe in variable framerate Update
        speedNow = speedNow* throttleKeptFromPrevFrame + throttleTarget * (1.0f- throttleKeptFromPrevFrame);
    }

    void Shoot()
    {
        GameObject shotGO = GameObject.Instantiate(rocketPrefab, fireFrom.position, transform.rotation);

        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out RaycastHit hit, range))
        { 
            Debug.Log(hit.transform.name);
            rocketPrefab.transform.Translate(hit.transform.position * Time.deltaTime);
        }

    }  
}
