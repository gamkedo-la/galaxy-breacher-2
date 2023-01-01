using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSelfRemove : MonoBehaviour
{
    [SerializeField] float destroyTimer = 2.0f;
    [SerializeField] GameObject ExplosionVFX;
    [SerializeField] Transform ExplosionPosition;
    public EnemyShipSpawnContoller enemyShipSpawnContoller;

    public void Remove()
    {
        GameObject explosion = Instantiate(ExplosionVFX, transform.position, Quaternion.identity);
        if (enemyShipSpawnContoller)
        {
            enemyShipSpawnContoller.RemoveShip();
        }
        Destroy(gameObject, destroyTimer);
    }
}
