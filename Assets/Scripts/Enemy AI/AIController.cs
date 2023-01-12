using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float chasingRange;

    [SerializeField] float rotationSpeed;
    void Start()
    {

    }

    void Update()
    {
        if (!GetIsInRange())
        {
            GetComponent<Movement>().MoveTo(target.position);
        }

        else
        {

            GetComponent<Movement>().Cancel();
        }

        LookAtPlayer();
    }

    private bool GetIsInRange()
    {
        return Vector3.Distance(transform.position, target.position) < chasingRange;
    }

    private void LookAtPlayer()
    {
        transform.LookAt(target.transform);

        Vector3 targetDirection = (target.position - transform.position).normalized;

        float targetAngleY = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
        float targetAngleZ = Mathf.Atan2(targetDirection.x, targetDirection.y) * Mathf.Rad2Deg;

        //using Mathf.MoveTowardsAngle to smoothly rotate the z-rotation value of the enemy ship towards the target angle
        float yRotation = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngleY, rotationSpeed * Time.deltaTime);
        float zRotation = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngleZ, rotationSpeed * Time.deltaTime);
        //assigning the new rotation to the enemy ship's transform
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, zRotation);
    }

}
