using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour
{
    [SerializeField] int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
