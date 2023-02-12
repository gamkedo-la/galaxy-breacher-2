using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigateToTask : Task
{
    private const float THRESHOLD_DISTANCE = 5f;

    private Ship ship;
    private Vector3 targetPosition;
    private Quaternion targetOrientation;
    private float maxSpeed;

    public NavigateToTask(Ship ship, Vector3 targetPosition, Quaternion targetOrientation)
        : this(ship, targetPosition, targetOrientation, ship.maxSpeed)
    {
    }

    public NavigateToTask(Ship ship, Vector3 targetPosition, Quaternion targetOrientation, float maxSpeed)
    {
        this.ship = ship;
        this.targetPosition = targetPosition;
        this.targetOrientation = targetOrientation;
        this.maxSpeed = maxSpeed;
    }

    public override void Start()
    {
        if(ship == null) {
            return;
        }
        base.Start();
        ship.NavigateTo(targetPosition, targetOrientation, maxSpeed);
    }

    public override void Cancel()
    {
        ship.Stop();
        base.Cancel();
    }

    public override void Update()
    {
        if ((Vector3.Distance(ship.transform.position, targetPosition) < THRESHOLD_DISTANCE))
        {
            Completed();
        }
    }

    public override string ToString()
    {
        return "NavigateToTask(" + targetPosition + ")";
    }
}
