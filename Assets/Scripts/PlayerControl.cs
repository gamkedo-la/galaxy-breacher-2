using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour {
    public static PlayerControl instance;
    public TextMeshProUGUI speedIndicator;
    public GameObject rocketPrefab;
    public GameObject blastPrefab;
    public Transform fireFromRocket;
    public Transform fireFromMachine1;
    public Transform fireFromMachine2;
    public GameObject explosionToSpawn;


    [SerializeField] float Rocketrange;
    [SerializeField] float machineRange;

    public AudioSource engineLoopWeak;
    public AudioSource engineLoopStrong;

    public Animator damageLighting;
    public AudioSource damageSound;

    private Rigidbody rb;
    private Camera fpsCamera;

    Color color;


    [SerializeField] float rollSpeed = 60.0f;
    [SerializeField] float pitchSpeed = 40.0f;
    [SerializeField] float strafeSpeed = 20.0f;
    
    [Space(10)]
    [Header("Laser Properties")]
    [SerializeField] LineRenderer laserLineRendererLeft;
    [SerializeField] LineRenderer laserLineRendererRight;
    [SerializeField] float laserFrameTime = .15f;
    [SerializeField] float laserRange = 2000f;
    [SerializeField] float laserDamagePerSecond = 1f;
    [SerializeField] float laserMaxHeat = 100f;
    [SerializeField] float heatGainRate = 40f;
    [SerializeField] float heatLossRate = 50f;
    [SerializeField] AudioClip laserSFX;
    private bool laserOverHeated = false;
    private float laserHeat = 0f;
    private bool leftLaserFiresNext = true;
    private float lastLaserSwitchTime = 0f;


    

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
        fpsCamera = Camera.main;
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

        if (Input.GetKeyDown(KeyCode.Space)) {

           ShootRocket();
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

        if (Input.GetMouseButtonDown(0))
        {
            ShootBlast();
        }

        UpdateLaser();

    }

    void FixedUpdate() { // using % per frame would be unsafe in variable framerate Update
        speedNow = speedNow* throttleKeptFromPrevFrame + throttleTarget * (1.0f- throttleKeptFromPrevFrame);
    }

    void ShootRocket()
    { 
        GameObject shotGO = GameObject.Instantiate(rocketPrefab, fireFromRocket.position, transform.rotation);

        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out RaycastHit hit, Rocketrange))
        { 
            Debug.Log(hit.transform.name);
            rocketPrefab.transform.Translate(hit.transform.position * Time.deltaTime);
        }

    }  

    void ShootBlast()
    {
        GameObject shotGO1 = GameObject.Instantiate(blastPrefab, fireFromMachine1.position, transform.rotation);
        GameObject shotGO2 = GameObject.Instantiate(blastPrefab, fireFromMachine2.position, transform.rotation);


        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out RaycastHit hit, machineRange))
        {
            Debug.Log(hit.transform.name);
            GameObject blastGO = GameObject.Instantiate(explosionToSpawn, hit.transform.position, hit.transform.rotation);
        }
    }

    void UpdateLaser()
    {
        //update laser start positions
        laserLineRendererLeft.SetPosition(0, laserLineRendererLeft.transform.position);
        laserLineRendererRight.SetPosition(0, laserLineRendererRight.transform.position);
        
        //if input and not over heated fire a laser
        if (Input.GetButton("Fire2") && !laserOverHeated)
        {
            //setup raycast
            Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
            Vector3 laserEndPosition;
            
            //TODO: Probably add a layer mask here so we only hit the things we want
            if (Physics.Raycast(ray, out RaycastHit hitInfo, laserRange))
            {
                laserEndPosition = hitInfo.point;
                //TODO: Deal damage to object
                //hitobject.TakeDamage(laserDamagePerSecond*Time.deltaTime);
            }
            else
            {
                laserEndPosition = fpsCamera.transform.position + fpsCamera.transform.forward * laserRange;
            }
            
            //set laser end positions based on which laser is firing
            if (leftLaserFiresNext)
            {
                laserLineRendererLeft.SetPosition(1, laserEndPosition);
                laserLineRendererRight.SetPosition(1, laserLineRendererRight.transform.position);
            }
            else
            {
                laserLineRendererLeft.SetPosition(1, laserLineRendererLeft.transform.position);
                laserLineRendererRight.SetPosition(1, laserEndPosition);
            }
            
            //switch which laser is firing based on time
            lastLaserSwitchTime += Time.deltaTime;
            if (lastLaserSwitchTime > laserFrameTime)
            {
                lastLaserSwitchTime -= laserFrameTime;
                leftLaserFiresNext = !leftLaserFiresNext;
            }
            
            //add laser heat and check overheated
            laserHeat += heatGainRate * Time.deltaTime;
            if (laserHeat > laserMaxHeat)
            {
                laserOverHeated = true;
            }
        }
        else //Don't fire a laser
        {
            //reduce laser heat and check overheated
            laserHeat -= heatLossRate * Time.deltaTime;
            if (laserHeat < 0)
            {
                laserHeat = 0;
                laserOverHeated = false;
            }
            //set laser end positions
            laserLineRendererLeft.SetPosition(1, laserLineRendererLeft.transform.position);
            laserLineRendererRight.SetPosition(1, laserLineRendererRight.transform.position);
        }
        
        //reset laser variables when releasing input
        if (Input.GetButtonUp("Fire2"))
        {
            lastLaserSwitchTime = 0;
            leftLaserFiresNext = !leftLaserFiresNext;
        }
        //TODO: Replace logging with UI
        Debug.Log("LaserHeat: " + laserHeat + " Overheated: " + laserOverHeated);
    }
}
