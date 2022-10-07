using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticLatch : MonoBehaviour
{

    [SerializeField] Rigidbody constrainedRigidBody;
    [SerializeField] Vector3 releaseForce;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.M)){
            constrainedRigidBody.constraints = 0b00;
            constrainedRigidBody.isKinematic = false;
            constrainedRigidBody.AddForce(constrainedRigidBody.transform.TransformVector(releaseForce), ForceMode.Impulse);
        }
    }
}
