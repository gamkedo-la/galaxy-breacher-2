using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 heading; // this comes by setting the 'TurretHeading' sphere in the direction you want the turret to face

    public float maxTurretAngle = 90.0f;

    private GameObject player = null;

    private GameObject turretHeadingPoint = null;

    private bool playerInRange;

    void Start()
    {
        if (player == null) {
            player = GameObject.Find("Player");
        }
        if (turretHeadingPoint == null) {
            turretHeadingPoint = GameObject.Find("TurretHeadingPoint");
        }
        heading = Vector3.Normalize(turretHeadingPoint.transform.position - transform.position);
        playerInRange = false;
    }

    // Update is called once per frame
    void Update()
    {
        turretHeadingPoint = GameObject.Find("TurretHeadingPoint");
        player = GameObject.Find("Player"); // not sure if this is the fast/proper way to do it
        heading = Vector3.Normalize(turretHeadingPoint.transform.position - transform.position); // this is probably unnecessary unless the turret is going to be moving around
        playerInRange = false;
        Vector3 playerPos = player.transform.position;
        Vector3 castToPlayer = playerPos - transform.position;
        // Debug.Log("heading is " + heading.x + " x " + heading.y + " y " + heading.z + " z");
        float innerProduct = Vector3.Dot(castToPlayer, heading);
        float angle =  Mathf.Rad2Deg * Mathf.Acos(innerProduct / (Vector3.Magnitude(heading) * Vector3.Magnitude(castToPlayer)));
        Debug.Log("angle between turrent and player is " + angle);
        if ((angle) > maxTurretAngle) {
            // Debug.Log("Can't hit shoot player");
        } else {
            // not taking into account actual visibility/occlusion yet
            playerInRange = true;
            // Debug.Log("Can hit player");
        }
    }
}
