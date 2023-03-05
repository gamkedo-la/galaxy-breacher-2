using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public GameObject winMessage;

    public PlayerShipUI healthShieldUI;

    private bool showPlayerWeaponDebug = false;

    [Space(10)]
    [Header("Health Properties")]
    public Animator damageLighting;
    private const float delayBetweenHarm = 0.6f;
    private float waitingBetweenDamage = 0.0f;

    [Space(10)]
    [Header("Movement Properties")]
    public bool invertPitchAxis = false;
    [SerializeField] float rollSpeed = 60.0f;
    [SerializeField] float pitchSpeed = 40.0f;
    [SerializeField] float strafeSpeed = 20.0f;
    float speedNow = 0.0f;
    float maxNegativeSpeed = -20.0f;
    float maxForwardSpeed = 60.0f;
    float forwardAccel = 29.0f; // affects Q/E manual adjustment
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

    private static uint engineSoundLoopID = 0;

    void Awake()
    {
        instance = this; // singleton for AI to aim etc
    }

    public static void StopEngineLoopIfPlaying() {
        if(engineSoundLoopID != 0) {
            AkSoundEngine.StopPlayingID(engineSoundLoopID, 500, AkCurveInterpolation.AkCurveInterpolation_Constant);
            engineSoundLoopID = 0;
        }
    }

    void Start()
    {
        engineSoundLoopID = AkSoundEngine.PostEvent("Player_Engine", gameObject);
        PlayMenuSong.SaveGameMusicAndPlayOneSongAtATime(AkSoundEngine.PostEvent("Game_Music", gameObject));
        AkSoundEngine.SetSwitch("Gameplay_Switch","Gameplay", gameObject);
        if (turretControlMode)
        {
            turretUpward = transform.forward;
        }

        rb = GetComponent<Rigidbody>();
        color = Color.red;
        fpsCamera = Camera.main;
        turretCount = -1;
        StartCoroutine(MGFireCheck());
        Cursor.lockState = CursorLockMode.Locked;
        SetPauseState(false); // hide panel if open at start
    }

    void RefreshEngineVolume()
    {

        if (turretControlMode)
        {
            // player ship doesn't move at this level, have engine idle at low volume
            //engineLoopWeak.volume = 1.0f;
            //engineLoopStrong.volume = 0f;
        }
        else
        {
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

    void SetPauseState(bool isPaused) {
        gamePaused = isPaused;
        pauseUI.active = gamePaused;
        gameplayUI.active = (gamePaused == false);
        if (gamePaused) {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        Time.timeScale = (gamePaused ? 0.0f : 1.0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            StopEngineLoopIfPlaying();
            SceneManager.LoadScene("LevelSelect");
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetPauseState(!gamePaused);
        }

        if (gamePaused)
        {
            return; // no control input handling while paused other than P toggle key
        }

        if (waitingBetweenDamage > 0.0f) {
            waitingBetweenDamage -= Time.deltaTime;
        }

        RefreshEngineVolume();

        bigShipExplosionProcess(); // all enemy parts removed? (ideally only checking after it changes, but, don't want to risk missing due to a bug)

        if (turretControlMode == false)
        {
            if (Cursor.lockState == CursorLockMode.Locked && Cursor.visible == false)
            {
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

            if (Mathf.Abs(speedNow) <= 1.0f) {
                speedIndicator.text = "E: Speed up / Q: Slow down";
            } else {
                speedIndicator.text = "Engine: " + Mathf.Round(speedNow / maxForwardSpeed * 100.0f) + "%";
            }
        }
        else
        {
            turretYaw += Input.GetAxis("Roll");
            turretYaw = Mathf.Clamp(turretYaw, -90f, 90f);
            turretPitch += Input.GetAxis("Pitch") * (invertPitchAxis ? 1.0f : -1.0f);
            turretPitch = Mathf.Clamp(turretPitch, -90f, 90f);
            transform.rotation = Quaternion.LookRotation(turretUpward)
                                 * Quaternion.AngleAxis(turretYaw, Vector3.up)
                                 * Quaternion.AngleAxis(turretPitch, Vector3.right);
            speedIndicator.text = "TURRET";
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootRocket();
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
            ReceiveDamagePaced();
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
        if (gamePaused)
        {
            return; // no updates while paused
        }
        speedNow = speedNow * throttleKeptFromPrevFrame + throttleTarget * (1.0f - throttleKeptFromPrevFrame);
    }

    void ShootRocket()
    {
        GameObject shotGO = GameObject.Instantiate(rocketPrefab, fireFromRocket.position, transform.rotation);

        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out RaycastHit hit, Rocketrange))
        {
            if (showPlayerWeaponDebug)
            {
                Debug.Log("Rocket tracking: " + hit.transform.name);
            }
            shotGO.transform.LookAt(hit.point);
        }
    }

    void ShootBlast()
    {
        GameObject shotGO1 = GameObject.Instantiate(blastPrefab, fireFromMachine1.position, transform.rotation);
        shotGO1.transform.SetParent(transform);
        AkSoundEngine.PostEvent("MachineGun", gameObject);

        GameObject shotGO2 = GameObject.Instantiate(blastPrefab, fireFromMachine2.position, transform.rotation);
        shotGO2.transform.SetParent(transform);


        if (Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out RaycastHit hit, machineRange))
        {
            IDamageable damageable = hit.collider.gameObject.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                if (hit.collider.tag == "Shield")
                {
                    if (showPlayerWeaponDebug)
                    {
                        Debug.Log("shield blocked MG");
                    }
                }
                else
                {
                    if (showPlayerWeaponDebug)
                    {
                        Debug.Log(hit.transform.name + " taking damage from MG");
                    }
                    damageable.TakeDamage(3); // careful, rapid fire, so small number difference is significant
                }
            }
            else if (showPlayerWeaponDebug)
            {
                Debug.Log("no damageable found for " + hit.transform.name);
            }
            GameObject blastGO = GameObject.Instantiate(explosionToSpawn, hit.point, Quaternion.identity);
            ExplosionSelfRemove esrScript = blastGO.GetComponent<ExplosionSelfRemove>();
            esrScript.ExplodeAndRemove();
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
                    IDamageable damageable = hitInfo.collider.gameObject.GetComponentInParent<IDamageable>();
                    if (damageable != null)
                    {
                        if (hitInfo.collider.tag == "Shield")
                        {
                            if (showPlayerWeaponDebug)
                            {
                                Debug.Log("shield blocked laser");
                            }
                        }
                        else
                        {
                            if (showPlayerWeaponDebug)
                            {
                                Debug.Log(hitInfo.transform.name + " taking damage from laser");
                            }
                            damageable.TakeDamage(laserDamagePerSecond / laserFireRate);
                        }
                    }
                    else if (showPlayerWeaponDebug)
                    {
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

    private bool bigShipExplodedYet = false;
    private void bigShipExplosionProcess()
    {
        if (bigShipExplodedYet == false &&
            (PlayerShipUI.instance.IsBossPartCountZero() || Input.GetKeyDown(KeyCode.U)))
        {
            Debug.Log("end of level!");
            bigShipExplodedYet = true; // prevents call more than once, especially to not stack coroutines
            StartCoroutine(ReturnAfterSuccess());
        }
    }

    IEnumerator ReturnAfterSuccess() {
        string sceneToLoadAfter;
        winMessage.SetActive(true);
        if (bigShipExplosion != null) {
            Instantiate(bigShipExplosion, bigShipExplosionPosition.transform.position, Quaternion.identity);
            Destroy(bigShip, 3f);
            sceneToLoadAfter = "LevelSelect";
        } else {
            sceneToLoadAfter = "StoryOutro";
        }
        yield return new WaitForSeconds(4.0f);
        StopEngineLoopIfPlaying();
        SceneManager.LoadScene(sceneToLoadAfter);
    }

    public void ReceiveDamagePaced()
    {
        if (waitingBetweenDamage > 0.0f)
        {
            return;
        }
        damageLighting.SetTrigger("Damage");
        damageSound.Play();

        waitingBetweenDamage = delayBetweenHarm;
        healthShieldUI.TakeDamage();
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("hit player: " + collision.gameObject.name);
        if (collision.gameObject.tag == "HullPickUp")
        {
            healthShieldUI.HealHull();
        }
        if (collision.gameObject.tag == "Astroid")
        {
            healthShieldUI.TakeDamage();
        }
    }

    public void SetInversion(bool inversion)
    {
        invertPitchAxis = inversion;
    }
}
