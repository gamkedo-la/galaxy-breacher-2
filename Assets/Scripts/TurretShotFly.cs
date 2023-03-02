using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShotFly : MonoBehaviour
{
    Rigidbody rb;
    float maxLifetimeInSec = 15.0f;

    float shotSpeed = 150.0f;
    void Start()
    {
        HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, maxLifetimeInSec);
    }

    // Update is called once per frame
    void Update()
    {
        // where does the position get updated?
        // rb.velocity = transform.forward * shotSpeed;
        transform.position += transform.forward * shotSpeed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision) {
        PlayerControl playerScript = collision.gameObject.GetComponent<PlayerControl>();
        if(playerScript) {
            Debug.Log("Player hit!");
            playerScript.ReceiveDamagePaced();
        }
        // presumably need to set some damage here if it hits the player?
        // it's colliding with the turrent atm on launch
        //Destroy(gameObject);
    }
}
