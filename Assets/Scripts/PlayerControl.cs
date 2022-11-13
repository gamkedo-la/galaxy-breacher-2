using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour {
    public static PlayerControl instance;
    
    [Space(10)]
    [Header("Health Properties")]
    public Animator damageLighting;
    
    [Space(10)]
    [Header("Movement Properties")]
    [SerializeField] float rollSpeed = 60.0f;
    [SerializeField] float pitchSpeed = 40.0f;
    [SerializeField] float strafeSpeed = 20.0f;
    float speedNow = 0.0f;
    float maxNegativeSpeed = -20.0f;
    float maxForwardSpeed = 60.0f;
    float forwardAccel = 9.0f; // affects Q/E manual adjustment
    float throttleKeptFromPrevFrame = 0.985f; // affects 1-4 presets
    float throttleTarget = 0.0f;
    
    [Space(10)]
    [Header("Rocket Properties")]
    [SerializeField] float Rocketrange;
    public Transform fireFromRocket;
    public GameObject rocketPrefab;
    
    [Space(10)]
    [Header("Machine Gun Properties")]
    public Transform fireFromMachine1;
    public Transform fireFromMachine2;
    public GameObject explosionToSpawn;
    [SerializeField] float machineRange;
    public GameObject blastPrefab;
    
    [Space(10)]
    [Header("Laser Properties")]
    [SerializeField] LineRenderer laserLineRendererLeft;
    [SerializeField] LineRenderer laserLineRendererRight;
    [SerializeField] float laserFireRate = 7f;
    [SerializeField] float laserRange = 2000f;
    [SerializeField] float laserDamagePerSecond = 1f;
    [SerializeField] float laserMaxHeat = 100f;
    [SerializeField] float heatGainRate = 40f;
    [SerializeField] float heatLossRate = 50f;

    private bool laserOverHeated = false;
    private float laserHeat = 0f;
    private bool leftLaserFiresNext = true;
    private float lastLaserFireTime = 0f;
    
    [Space(10)]
    [Header("Audio Properties")]
    public AudioSource engineLoopWeak;
    public AudioSource engineLoopStrong;
    public AudioSource damageSound;
    [SerializeField] AudioClip laserSFX;
    [SerializeField] AudioClip laserOverheatSFX;
    [SerializeField] AudioClip laserCooldownSFX;
    [SerializeField] AudioSource laserAudioSource;
    
    [Space(10)]
    [Header("UI Properties")]
    [SerializeField] LaserHeatUI laserUI;
    public TextMeshProUGUI speedIndicator;
    
    private Rigidbody rb;
    private Camera fpsCamera;
    Color color;
    
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
        //update last fired time
        lastLaserFireTime += Time.deltaTime;
        
        //update laser start positions
        laserLineRendererLeft.SetPosition(0, laserLineRendererLeft.transform.position);
        laserLineRendererRight.SetPosition(0, laserLineRendererRight.transform.position);

        if (Input.GetButton("Fire2") && !laserOverHeated)
        {
            //Check if we hit something
            Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
            //TODO: Probably add a layer mask here so we only hit the things we want
            bool laserHit = Physics.Raycast(ray, out RaycastHit hitInfo, laserRange);

            //if it's time to fire another laser
            if (lastLaserFireTime > (1/laserFireRate))
            {
                lastLaserFireTime = 0;
                //switch which laser to fire
                leftLaserFiresNext = !leftLaserFiresNext;
                
                //Deal Damage
                if (laserHit)
                {
                    IDamageable damageableObject = hitInfo.collider.GetComponent<IDamageable>();
                    if (damageableObject != null)
                    {
                        damageableObject.TakeDamage(laserDamagePerSecond/laserFireRate);
                    }
                }

                //play sfx on correct side
                if (leftLaserFiresNext)
                {
                    laserAudioSource.panStereo = -.5f;
                    laserAudioSource.PlayOneShot(laserSFX);
                }
                else
                {
                    laserAudioSource.panStereo = .5f;
                    laserAudioSource.PlayOneShot(laserSFX);
                }
                //increase laser heat
                laserHeat += heatGainRate / laserFireRate;
                if (laserHeat > laserMaxHeat)
                {
                    laserOverHeated = true;
                    laserAudioSource.panStereo = 0f;
                    laserAudioSource.PlayOneShot(laserOverheatSFX);
                }
            }
            //set laser end to hit target or max range
            Vector3 laserEndPosition = laserHit
                ? hitInfo.point
                : fpsCamera.transform.position + fpsCamera.transform.forward * laserRange;
            
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
        }else //Don't fire a laser
        {
            //set laser end positions
            laserLineRendererLeft.SetPosition(1, laserLineRendererLeft.transform.position);
            laserLineRendererRight.SetPosition(1, laserLineRendererRight.transform.position);
        }
        //reduce laser heat and check overheated
        laserHeat -= heatLossRate * Time.deltaTime;
        if (laserHeat < 0)
        {
            laserHeat = 0;
            if (laserOverHeated)
            {
                laserOverHeated = false;
                laserAudioSource.panStereo = 0f;
                laserAudioSource.PlayOneShot(laserCooldownSFX);
            }
        }
        laserUI.SetHeatPercentage(laserHeat/laserMaxHeat);
    }
}
