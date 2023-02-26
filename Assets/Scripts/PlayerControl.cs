using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl instance;

    // for special stage where player controls turret instead of ship
    // can't strafe, no throttle, and angles are more pivot driven
    public bool turretControlMode = false;
    public float turretYaw = 0.0f;
    public float turretPitch = 45.0f;
    private Vector3 turretUpward;

    public PlayerShipUI healthShieldUI;

    [Space(10)]
    [Header("Health Properties")]
    public Animator damageLighting;

    [Space(10)]
    [Header("Movement Properties")]
    public bool invertPitchAxis = false;
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
    private bool firingMG;

    [Space(10)]
    [Header("Small Ship Explosion Properties")]
    public float maximumDistance;
    public float radius;

    [Space(10)]
    [Header("Big Ship Explosion Properties")]

    [SerializeField] GameObject bigShip;
    [SerializeField] GameObject turretExplosion;
    [SerializeField] GameObject bigShipExplosion;
    [SerializeField]
    GameObject bigShipExplosionPosition;
    [SerializeField] List<GameObject> turrets = new List<GameObject>();
    int turretCount;
    [SerializeField] List<GameObject> spawnPoints = new List<GameObject>();
    int spawnPointCount;


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
    
    public AudioSource damageSound;

    [SerializeField] AudioClip laserSFX;
    [SerializeField] AudioClip laserOverheatSFX;
    [SerializeField] AudioClip laserCooldownSFX;
    [SerializeField] AudioSource laserAudioSource;

    [Space(10)]
    [Header("UI Properties")]
    [SerializeField] LaserHeatUI laserUI;
    public TextMeshProUGUI speedIndicator;
    public GameObject gameplayUI; // to toggle off for showing pause
    public GameObject pauseUI;
    private bool gamePaused = false;

    private Rigidbody rb;
    private Camera fpsCamera;
    Color color;

    void Awake()
    {
        instance = this; // singleton for AI to aim etc
    }

    void Start() { 
        AkSoundEngine.PostEvent("Player_Engine" ,gameObject);
         if(turretControlMode) {
            turretUpward = transform.forward;
        }

        rb = GetComponent<Rigidbody>();
        color = Color.red;
        fpsCamera = Camera.main;
        turretCount = -1;
        StartCoroutine(MGFireCheck());
        Cursor.lockState = CursorLockMode.Locked;
    }

    void RefreshEngineVolume()
    {

        if(turretControlMode) {
            // player ship doesn't move at this level, have engine idle at low volume
            //engineLoopWeak.volume = 1.0f;
            //engineLoopStrong.volume = 0f;
        } else {
            float enginePowerFade = Mathf.Abs(speedNow / maxForwardSpeed); // velocity as main component

            float strafeEngineEffect = 0.55f * (
                Mathf.Abs(Input.GetAxis("Horizontal")) +
                Mathf.Abs(Input.GetAxis("Vertical")));

            float forwardOrStrafeEngineBalance = 0.6f;
            enginePowerFade = forwardOrStrafeEngineBalance * enginePowerFade + (1.0f - forwardOrStrafeEngineBalance) * strafeEngineEffect;
            enginePowerFade = Mathf.Clamp(enginePowerFade, 0.0f, 1.0f);
            AkSoundEngine.SetRTPCValue("Player_Engine_Strength", enginePowerFade, gameObject);
            AkSoundEngine.SetRTPCValue("Player_Engine_Strength_2", enginePowerFade, gameObject);
            
            //set weak/strong param in wwise event based on engine power fade (0 to 1)
            //engineLoopWeak.volume = 1.0f - enginePowerFade;
            //engineLoopStrong.volume = enginePowerFade;

        }
    }

    IEnumerator MGFireCheck()
    {
        while (true)
        {
            if (firingMG)
            {
                ShootBlast();
            }
            yield return new WaitForSeconds(0.175f);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            gamePaused = !gamePaused;
            pauseUI.active = gamePaused;
            gameplayUI.active = (gamePaused == false);
            if(gamePaused) {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            Time.timeScale = (gamePaused ? 0.0f : 1.0f);
        }

        if(gamePaused) {
            return; // no control input handling while paused other than P toggle key
        }

        RefreshEngineVolume();

        if (turretControlMode == false) {
            if (Cursor.lockState == CursorLockMode.Locked && Cursor.visible == false) {
                transform.Rotate(Vector3.forward, Input.GetAxis("Roll") * -rollSpeed * Time.deltaTime);
                transform.Rotate(Vector3.right, (invertPitchAxis ? 1.0f : -1.0f) * Input.GetAxis("Pitch") * pitchSpeed * Time.deltaTime);
            }
            if (Mathf.Abs(Input.GetAxis("Throttle")) > 0.15f)
            { // manual throttle overrides 1-4 presets
                speedNow += Input.GetAxis("Throttle") * forwardAccel * Time.deltaTime;
                throttleTarget = speedNow;
            }

            speedNow = Mathf.Clamp(speedNow, maxNegativeSpeed, maxForwardSpeed);

            Vector3 moveVec = Vector3.zero;

            moveVec += transform.right * Input.GetAxis("Horizontal") * strafeSpeed;
            moveVec += transform.up * Input.GetAxis("Vertical") * strafeSpeed;
            moveVec += transform.forward * speedNow;
            // remember: no  * Time.deltaTime here since the .velocity of rb already handles that
            rb.velocity = moveVec;

            speedIndicator.text = "Speed: " + Mathf.Round(speedNow * 100.0f);
        } else {
            turretYaw += Input.GetAxis("Roll");
            turretYaw = Mathf.Clamp(turretYaw,-90f,90f);
            turretPitch += Input.GetAxis("Pitch") * (invertPitchAxis ? 1.0f : -1.0f);
            turretPitch = Mathf.Clamp(turretPitch, -90f, 90f);
            transform.rotation = Quaternion.LookRotation(turretUpward)
                                 * Quaternion.AngleAxis(turretYaw,Vector3.up)
                                 * Quaternion.AngleAxis(turretPitch, Vector3.right);
            speedIndicator.text = "TURRET";
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootRocket();
            smallShipExplosionProcess();
            bigShipExplosionProcess();
        }

        if (Input.GetButtonDown("Engine-Off"))
        {
            throttleTarget = 0.0f;
        }
        if (Input.GetButtonDown("Engine-Low"))
        {
            throttleTarget = maxForwardSpeed * 0.3f;
        }
        if (Input.GetButtonDown("Engine-Mid"))
        {
            throttleTarget = maxForwardSpeed * 0.6f;
        }
        if (Input.GetButtonDown("Engine-Max"))
        {
            throttleTarget = maxForwardSpeed * 1.0f;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            AkSoundEngine.PostEvent("Player_Cabin_Warning", gameObject);
            print("Take Damage Lights: Eventually move this to where player loses health");
            damageLighting.SetTrigger("Damage");
            damageSound.Play();
        }

        bool wasFiring = firingMG;
        firingMG = Input.GetMouseButton(0); // no ammo etc check at this time
        if (firingMG && wasFiring == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            ShootBlast(); // instant extra shot on click, separate from rate limited coroutine loop
        }


        UpdateLaser();

    }

    void FixedUpdate()
    { // using % per frame would be unsafe in variable framerate Update
        if (gamePaused) {
            return; // no updates while paused
        }
        speedNow = speedNow * throttleKeptFromPrevFrame + throttleTarget * (1.0f - throttleKeptFromPrevFrame);
    }

    void ShootRocket()
    {
        GameObject shotGO = GameObject.Instantiate(rocketPrefab, fireFromRocket.position, transform.rotation);

        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out RaycastHit hit, Rocketrange))
        {
            Debug.Log(hit.transform.name);
            shotGO.transform.Translate(hit.transform.position * Time.deltaTime);
        }
    }

    void ShootBlast()
    {
        GameObject shotGO1 = GameObject.Instantiate(blastPrefab, fireFromMachine1.position, transform.rotation);
        shotGO1.transform.SetParent(transform);
        AkSoundEngine.PostEvent("MachineGun" ,gameObject);
      
        GameObject shotGO2 = GameObject.Instantiate(blastPrefab, fireFromMachine2.position, transform.rotation);
        shotGO2.transform.SetParent(transform);


        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out RaycastHit hit, machineRange))
        {
            IDamageable damageable = hit.collider.gameObject.GetComponent<IDamageable>();
            if (damageable != null) {
                Debug.Log(hit.transform.name + " taking damage from MG");
                damageable.TakeDamage(1); // smallest damage increment
            } else {
                Debug.Log("no damageable found for " + hit.transform.name);
            }
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
            if (lastLaserFireTime > (1 / laserFireRate))
            {
                lastLaserFireTime = 0;
                //switch which laser to fire
                leftLaserFiresNext = !leftLaserFiresNext;

                //Deal Damage
                if (laserHit)
                {
                    IDamageable damageable = hitInfo.collider.gameObject.GetComponent<IDamageable>();
                    if (damageable != null) {
                        Debug.Log(hitInfo.transform.name + " taking damage from laser");
                        damageable.TakeDamage(laserDamagePerSecond / laserFireRate);
                    } else {
                        Debug.Log("no damageable found for " + hitInfo.transform.name);
                    }
                }

                //play sfx on correct side
                if (leftLaserFiresNext)
                {
                    laserAudioSource.panStereo = -.5f;
                    laserAudioSource.PlayOneShot(laserSFX);
                    AkSoundEngine.SetRTPCValue("Laser_Panning", 25, gameObject);
                    AkSoundEngine.PostEvent("Player_Laser", gameObject);
                }
                else
                {
                    laserAudioSource.panStereo = .5f;
                    laserAudioSource.PlayOneShot(laserSFX);
                    AkSoundEngine.SetRTPCValue("Laser_Panning", 75, gameObject);
                    AkSoundEngine.PostEvent("Player_Laser", gameObject);
                }
                //increase laser heat
                laserHeat += heatGainRate / laserFireRate;
                if (laserHeat > laserMaxHeat)
                {
                    laserOverHeated = true;
                    laserAudioSource.panStereo = 0f;
                    laserAudioSource.PlayOneShot(laserOverheatSFX);
                    AkSoundEngine.PostEvent("Laser_OverHeat", gameObject);
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
        }
        else //Don't fire a laser
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
        laserUI.SetHeatPercentage(laserHeat / laserMaxHeat);
    }

    private void smallShipExplosionProcess()
    {
        RaycastHit hit;

        if (Physics.SphereCast(fpsCamera.transform.position, radius, fpsCamera.transform.forward, out hit, maximumDistance))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Destroyable"))
            {
                hit.transform.gameObject.GetComponentInParent<ExplosionSelfRemove>().Remove();
            }
        }
    }

    private void bigShipExplosionProcess()
    {
        RaycastHit hit;

        if (Physics.SphereCast(fpsCamera.transform.position, radius, fpsCamera.transform.forward, out hit, maximumDistance))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Turret"))
            {
                Destroy(hit.transform.parent.gameObject, 6f);
                turretCount++;
                Debug.Log(turretCount);
            }

            if (hit.transform.CompareTag("SpawnPoint"))
            {
                //Can be changed later to not to destroy but swap out with another object.
                Destroy(hit.transform.parent.gameObject, 6f);
                spawnPointCount++;
                Debug.Log(spawnPointCount);
            }

            if (turretCount >= turrets.Count && spawnPointCount >= spawnPoints.Count)
            {
                Instantiate(bigShipExplosion, bigShipExplosion.transform.position, Quaternion.identity);
                Destroy(bigShip, 3f);
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log("hit player: " + collision.gameObject.name);
        if (collision.gameObject.tag == "HullPickUp") {
            healthShieldUI.HealHull();
        }
        if (collision.gameObject.tag == "Astroid") {
            healthShieldUI.TakeDamage();
        }
    }
}
