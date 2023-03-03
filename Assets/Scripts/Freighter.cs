using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freighter : MonoBehaviour, IDamageable
{

    MagneticLatch[] latches;
    [SerializeField] float startHealth = 50;
    float health;
    ConstantForwardMovement constantForwardMovement;

    bool cargoReleased = false;

    // Start is called before the first frame update
    void Start()
    {
        latches = GetComponentsInChildren<MagneticLatch>();
        health = startHealth;
        constantForwardMovement = GetComponent<ConstantForwardMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if(health < 0 && !cargoReleased){
            ReleaseCargo();
        }
    }

    private void ReleaseCargo()
    {
        foreach (var latch in latches)
        {
            latch.Release();
        }
        constantForwardMovement.Speed = 2 * constantForwardMovement.Speed;
        cargoReleased = true;
        TradeInterdictionSceneController.Instance.FreighterReleasedCargo(this);
        var turrets = GetComponentsInChildren<TurretBehaviour>();
        foreach (var turret in turrets)
        {
            turret.Deactivate();
        }
    }

    public void TakeDamage(float damageToTake)
    {
        health -= damageToTake;
    }
}
