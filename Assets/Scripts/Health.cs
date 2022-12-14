using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private GameObject deathExplosion;
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
        if (deathExplosion)
        {
            Instantiate(deathExplosion, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No VFX for death of object: " + this.name);
        }

        Destroy(gameObject);
    }
}
