using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShipSpawnContoller : MonoBehaviour
{
    [Header("Spawn Objects")]
    public GameObject shipToSpawn;
    public Transform spawnParent;//since this will be attached to the main ship, and main ship will be able to move. We should have a separate parent.
    public GameObject spawnPoint;

    [Header("Spawn Parameters")]
    public float secondsBetweenEverySpawn = 1.0f;
    public int maximumNumberOfShips = 3;
    public int currentNumberOfShips = 0;

    private float timePassedAfterPreviousSpawn = 0.0f;

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
        CheckForSpawnShip();
    }

    private void CheckForSpawnShip()
    {
        if (timePassedAfterPreviousSpawn == secondsBetweenEverySpawn && currentNumberOfShips < maximumNumberOfShips)
        {
            SpawnShip();
        }
    }

    private void UpdateTime()
    {
        if (timePassedAfterPreviousSpawn >= secondsBetweenEverySpawn)
        {
            timePassedAfterPreviousSpawn = secondsBetweenEverySpawn;
        }
        else
        {
            timePassedAfterPreviousSpawn += Time.deltaTime;
        }
    }

    private void SpawnShip()
    {
        GameObject spawnedShip = Instantiate(shipToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation, spawnParent);
        ExplosionSelfRemove explosionSelfRemove;
        spawnedShip.TryGetComponent(out explosionSelfRemove);
        if (explosionSelfRemove)
        {
            explosionSelfRemove.enemyShipSpawnContoller = this;
        }
        currentNumberOfShips++;
        timePassedAfterPreviousSpawn = 0.0f;
    }

    public void RemoveShip()
    {
        if (currentNumberOfShips > 0)
        {
            currentNumberOfShips--;
            timePassedAfterPreviousSpawn = 0.0f;
        }
    }
}
