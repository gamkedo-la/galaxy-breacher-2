using UnityEngine;

public class PlayerMissileFly : MonoBehaviour
{
    public GameObject explosionToSpawn;
    public Transform effectTrailToRelease;
    Rigidbody rb;
    public float rocketSpeed = 50.0f;
    float maxLifetimeInSec = 30.0f;
    PlayerControl control;

    private void Start() {
        HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, maxLifetimeInSec);
        control = FindObjectOfType<PlayerControl>();
    }

    void Update()
    {   
        rb.velocity = control.transform.forward * rocketSpeed;

    }

    void OnCollisionEnter(Collision collision) {
        GameObject blastGO = GameObject.Instantiate(explosionToSpawn, transform.position, transform.rotation);
        Debug.Log("rocket hit " + collision.gameObject.name);
        HierarchyTrashSingleton.instance.GroupTempJunk(effectTrailToRelease);
        effectTrailToRelease.GetComponent<ParticleSystem>().Stop();
        Destroy(gameObject);
    }
}
