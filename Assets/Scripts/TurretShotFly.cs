using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShotFly : MonoBehaviour
{
    Rigidbody rb;
    float maxLifetimeInSec = 15.0f;

    float shotSpeed = 25.0f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, maxLifetimeInSec);
    }

    // Update is called once per frame
    void Update()
    {
        // where does the position get updated?
        rb.velocity = transform.forward * shotSpeed;
    }

    void OnCollisionEnter(Collision collision) {
        // presumably need to set some damage here if it hits the player?
        //Destroy(gameObject);
    }
}
