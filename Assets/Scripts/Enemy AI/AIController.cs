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

        if (!GetIsInRange())
        {
            GetComponent<Movement>().StartMoveAction(target.position);
        }

        else
        {

            GetComponent<Movement>().Cancel();
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
