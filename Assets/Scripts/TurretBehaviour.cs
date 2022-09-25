using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 heading;

    public float fovx;
    public float fovy;

    private GameObject player = null;

    private GameObject turretHeadingPoint = null;

    void Start()
    {
        if (player == null) {
            player = GameObject.Find("Player");
        }
        if (turretHeadingPoint == null) {
            turretHeadingPoint = GameObject.Find("TurretHeadingPoint");
        }
        heading = Vector3.Normalize(turretHeadingPoint.transform.position - transform.position);
       
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPos = player.transform.position;
        Debug.Log("heading is " + heading.x + " x " + heading.y + " y " + heading.z + " z");
    }
}
