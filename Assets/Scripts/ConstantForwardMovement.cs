using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantForwardMovement : MonoBehaviour
{

    Rigidbody rb;

    [SerializeField] float speed = 5f;
    
    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        rb.MovePosition(transform.position + (transform.forward * speed * Time.fixedDeltaTime));
    }
}
