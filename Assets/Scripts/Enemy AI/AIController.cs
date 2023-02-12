using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public Transform target;
    [SerializeField] float chasingRange;

    [SerializeField] float rotationSpeed;


    void Update()
    {
        if (target == null)
        {
            return; // prevennting error spam
        }
        MoveWhenOutOfRange();
        // LookAtPlayer();
    }

    private void MoveWhenOutOfRange()
    {
        Movement moveScript = GetComponent<Movement>();
        if(moveScript == null || target == null) {
            return; // suppress constant error from missing script/target
        }
        if (!GetIsInRange())
        {
            moveScript.StartMoveAction(target.position);
        }

        else
        {

            moveScript.Cancel();
        }

    }

    private bool GetIsInRange()
    {
        if (target == null)
        {
            return false; // to avoid error spam
        }
        return Vector3.Distance(transform.position, target.position) < chasingRange;
    }



}
