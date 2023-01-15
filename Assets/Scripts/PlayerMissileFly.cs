using UnityEngine;

public class PlayerMissileFly : MonoBehaviour
{
    public GameObject explosionToSpawn;
    public Transform effectTrailToRelease;
    Rigidbody rb;
    public float rocketSpeed = 50.0f;
    public float damage = 100f;
    float maxLifetimeInSec = 30.0f;
    PlayerControl control;

    private bool spawnedInShield = false;

    private void Start() {
        HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, maxLifetimeInSec);
        control = FindObjectOfType<PlayerControl>();

        if (Physics.CheckSphere(transform.position, 1.0f, LayerMask.GetMask("ShieldWontBlockShips")))
        {
            spawnedInShield = true;
        }
    }

    void Update()
    {   
        rb.velocity = control.transform.forward * rocketSpeed;

    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Shield" && spawnedInShield)
        {
            return;
        }
        GameObject blastGO = GameObject.Instantiate(explosionToSpawn, transform.position, transform.rotation);
        Debug.Log("rocket hit " + collision.gameObject.name);
        HierarchyTrashSingleton.instance.GroupTempJunk(effectTrailToRelease);
        effectTrailToRelease.GetComponent<ParticleSystem>().Stop();
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
