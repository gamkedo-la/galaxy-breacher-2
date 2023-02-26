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

    void OnCollisionEnter(Collision collision) {
        // this part doesn't block the missile, it allows it through if fired from inside it
        if(spawnedInShield && collision.gameObject.tag == "Shield") {
            return;
        }
        GameObject blastGO = GameObject.Instantiate(explosionToSpawn, transform.position, transform.rotation);
        if(showWeaponDebug) {
            Debug.Log("rocket hit " + collision.gameObject.name);
        }
        AkSoundEngine.PostEvent("ShipHit", gameObject);
        HierarchyTrashSingleton.instance.GroupTempJunk(effectTrailToRelease);
        effectTrailToRelease.GetComponent<ParticleSystem>().Stop();

        if (collision.gameObject.tag == "Shield")
        {
            if (showWeaponDebug) {
                Debug.Log("blocked by shield");
            }
            return;
        } else {
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
        Destroy(gameObject);
    }
}
