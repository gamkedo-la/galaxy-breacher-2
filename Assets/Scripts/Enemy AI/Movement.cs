using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour, IAction
{
    [SerializeField] float speed;
    bool stopFollowing;

    private void Awake()
    {
        stopFollowing = false;
    }

    public void StartMoveAction(Vector3 destination)
    {
        GetComponent<ActionSchedular>().StartAction(this);
        MoveTo(destination);
        stopFollowing = false;
    }

    public void Cancel()
    {
        stopFollowing = true;
    }

    public void MoveTo(Vector3 destination)
    {
        if (stopFollowing != true)
        {
            // Calculate the direction of the player ship
            Vector3 direction = destination - transform.position;

            // Normalize the direction
            direction = direction.normalized;

            // Move the enemy ship towards the player ship
            transform.position = Vector3.Lerp(transform.position, transform.position + (direction * speed * Time.deltaTime), Time.deltaTime * speed);
        }
    }
}
