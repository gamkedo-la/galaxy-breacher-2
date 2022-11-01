using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunBlast : MonoBehaviour
{
    public GameObject explosionToSpawn;
    Rigidbody rb;
   // public float blastSpeed = 2.0f;

    private void Start()
    {
        HierarchyTrashSingleton.instance.GroupTempJunk(transform);
       // rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject blastGO = GameObject.Instantiate(explosionToSpawn, transform.position, transform.rotation);
        Debug.Log(" hit " + collision.gameObject.name);
        Destroy(gameObject);
    }
}
