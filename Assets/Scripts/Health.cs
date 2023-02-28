using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float health;

    private void Start()
    {
        health = maxHealth;
    }


    public void TakeDamage(float damageToTake)
    {
        health -= damageToTake;
        if (health < 0)
        {
            Die();
        }
    }

    public void Die()
    {
        ExplosionSelfRemove esrScript = gameObject.GetComponent<ExplosionSelfRemove>();
        if (esrScript) {
            esrScript.Remove();
        }
        else {
            Debug.LogWarning("no explosion self remove for effect on " + gameObject.name);
        }
        Destroy(gameObject);
    }
}
