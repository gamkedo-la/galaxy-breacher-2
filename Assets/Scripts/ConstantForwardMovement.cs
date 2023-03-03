using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantForwardMovement : MonoBehaviour
{

    Rigidbody rb;
    PlayerControl playerControl;

    [SerializeField] float speed = 5f;

    public float Speed { get => speed; set => speed = value; }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {

        rb.MovePosition(transform.position + ( transform.forward * speed * Time.fixedDeltaTime));
    }
}
