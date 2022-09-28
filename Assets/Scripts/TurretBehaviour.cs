using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 heading; // this comes by setting the 'TurretHeading' sphere in the direction you want the turret to face

    public float maxTurretAngle = 90.0f;

    

    public GameObject turretShotPrefab;

    private bool playerInRange;

    float timeBetweenShots = 0.75f;
    private float aimSpeed = 0.035f; // percent per fixed update frame
    private float shotTimer;

    private bool canShoot;

    void Start()
    {
        canShoot = true;
        shotTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // do a bunch of work to a get a valid range for shooting
        heading = transform.forward;
        playerInRange = false;        
        Vector3 dirToPlayer = PlayerControl.instance.transform.position - transform.position;
        // Debug.Log("heading is " + heading.x + " x " + heading.y + " y " + heading.z + " z");
        float angle = Vector3.Angle(dirToPlayer, heading);
        // Debug.Log("angle between turrent and player is " + angle);
        if ((angle) > maxTurretAngle) {
            // Debug.Log("Can't hit shoot player");
        } else {
            // not taking into account actual visibility/occlusion yet
            playerInRange = true;
            // Debug.Log("Can hit player");
        }

        if (playerInRange && canShoot) {
            // actually shoot
            // gonna need a proper direction (that of the player) to pass as a Quaternion rotation?
            // Debug.Log("launching shot");
            canShoot = false;
            GameObject shotGO = GameObject.Instantiate(turretShotPrefab, transform.position, transform.rotation);
        }

        if (!canShoot) {
            shotTimer += Time.deltaTime;
            if (shotTimer >= timeBetweenShots) {
                canShoot = true;
                shotTimer = 0.0f;
            }
        }
    }

    void FixedUpdate() // slerp uses % so can't be in update which is framerate dependent
    {
        Vector3 dirToPlayer = PlayerControl.instance.transform.position - transform.position;
        Quaternion targetAng = Quaternion.LookRotation(dirToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetAng, aimSpeed);
    }
}


