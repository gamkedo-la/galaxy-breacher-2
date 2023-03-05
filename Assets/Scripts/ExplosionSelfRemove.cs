using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSelfRemove : MonoBehaviour
{
    [SerializeField] float destroyTimer = 2.0f;
    [SerializeField] GameObject ExplosionVFX;
    [SerializeField] Transform ExplosionPosition;
    public EnemyShipSpawnContoller enemyShipSpawnContoller;
    public string soundToPlayWithVFX = "ShipExplode";

    private bool alreadyDestroyedButNotRemovedYet = false; // helps boss tally update before GameObject is erased

    public void Start() {
        if(HierarchyTrashSingleton.instance) {
            HierarchyTrashSingleton.instance.GroupTempJunk(transform);
        }
        ExplosionSelfRemove[] allESR = gameObject.GetComponents<ExplosionSelfRemove>();
        if(allESR.Length > 1) {
            Debug.LogWarning("INSTANCE HAS MORE THAN 1 ESR ON IT: " + gameObject.name);
        }
    }

    public bool AlreadyRemoved() {
        return alreadyDestroyedButNotRemovedYet;
    }

    public void ExplodeAndRemove()
    {
        Debug.Log(gameObject.name + " reached ExplodeAndRemove");
        if(alreadyDestroyedButNotRemovedYet) {
            return; // prevent this somehow getting reached more than once (shouldn't)
        }

        alreadyDestroyedButNotRemovedYet = true;
        if (ExplosionVFX != null) {
            AkSoundEngine.PostEvent(soundToPlayWithVFX,gameObject);
            GameObject explosion = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        }
        
        if (enemyShipSpawnContoller)
        {
            enemyShipSpawnContoller.RemoveShip();
        }
        Destroy(gameObject, destroyTimer);

        if(transform.tag == "BossPart" ||transform.tag == "SpawnPoint") {
            Debug.Log("updating boss part count:");
            if (PlayerShipUI.instance) {
                PlayerShipUI.instance.UpdateBossPartCount();
            }
        }
    }
}
