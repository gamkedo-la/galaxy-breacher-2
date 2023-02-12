using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Navigation;


public class Movement : MonoBehaviour, IAction
{
    [SerializeField] float speed;
    Ship ship;
    bool stopFollowing;

    private void Awake()
    {
        stopFollowing = false;
        ship = GetComponent<Ship>();
    }

    public void StartMoveAction(Vector3 destination)
    {

        GetComponent<ActionSchedular>().StartAction(this);
        MoveTo(destination);



    }

    public void Cancel()
    {
        ship.Stop();
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if(ship == null) {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
        ship.NavigateTo(targetPosition, targetRotation, speed);



    }
}
