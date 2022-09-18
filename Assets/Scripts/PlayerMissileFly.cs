using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissileFly : MonoBehaviour
{
    public GameObject explosionToSpawn;
    public Transform effectTrailToRelease;
    Rigidbody rb;
    float rocketSpeed = 50.0f;
    float maxLifetimeInSec = 15.0f;

    private void Start() {
        HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, maxLifetimeInSec);
    }

    void Update()
    {
        rb.velocity = transform.forward * rocketSpeed;        
    }

    void OnCollisionEnter(Collision collision) {
        GameObject blastGO = GameObject.Instantiate(explosionToSpawn, transform.position, transform.rotation);
        Debug.Log("rocket hit " + collision.gameObject.name);
        HierarchyTrashSingleton.instance.GroupTempJunk(effectTrailToRelease);
        Destroy(gameObject);
    }
}
