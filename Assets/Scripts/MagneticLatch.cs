using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticLatch : MonoBehaviour
{

    [SerializeField] Rigidbody constrainedRigidBody;
    [SerializeField] Vector3 releaseForce;
    

    public void Release(){
        constrainedRigidBody.constraints = 0b00;
            constrainedRigidBody.isKinematic = false;
            constrainedRigidBody.AddExplosionForce(releaseForce.y, transform.parent.position, 1000f,0f, ForceMode.Impulse);
            //constrainedRigidBody.AddForce(constrainedRigidBody.transform.TransformVector(releaseForce), ForceMode.Impulse);
            AkSoundEngine.PostEvent("LatchRelease", gameObject);
    }
}
