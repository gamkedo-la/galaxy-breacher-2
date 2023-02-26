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
    private bool showWeaponDebug = false;

    private void Start() {
        AkSoundEngine.PostEvent("PlayerMissile", gameObject);
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

    void OnTriggerExit(Collider other) {
        spawnedInShield = false; // if we fire rocket out from shield another shield should stop it
        if (showWeaponDebug) {
            Debug.Log("rocket exited the shield it was spawned in");
        }
    }
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Shield" && spawnedInShield == false) {
            if (showWeaponDebug) {
                Debug.Log("blocked by shield");
            }
            ExplodeMissile();
        }
    }

    void ExplodeMissile() { // note: doesn't apply damage
        GameObject blastGO = GameObject.Instantiate(explosionToSpawn, transform.position, transform.rotation);
        AkSoundEngine.PostEvent("ShipHit", gameObject);
        HierarchyTrashSingleton.instance.GroupTempJunk(effectTrailToRelease);
        effectTrailToRelease.GetComponent<ParticleSystem>().Stop();
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision) {
        if(showWeaponDebug) {
            Debug.Log("rocket hit " + collision.gameObject.name);
        }

        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
        if (damageable != null) {
            if (showWeaponDebug) {
                Debug.Log("Taking missile damage: " + collision.gameObject.name);
            }
            damageable.TakeDamage(damage);
        } else if (showWeaponDebug) {
            Debug.Log("No Damageable for missile to affect: " + collision.gameObject.name);
        }
    }
}
