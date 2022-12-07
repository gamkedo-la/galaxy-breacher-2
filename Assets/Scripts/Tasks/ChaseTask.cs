using UnityEngine;

public class ChaseTask : Task
{
    const float REPLAN_THRESHOLD = 10f;

    private Ship ship;
    private Transform target;

    private Task currentNavigateTask;
    private Vector3 lastPosition;

    public ChaseTask(Ship ship, Transform target)
    {
        this.ship = ship;
        this.target = target;
    }

    public override void Start()
    {
        base.Start();
        currentNavigateTask = new NavigateToTask(ship, target.position, Quaternion.identity);
        currentNavigateTask.Start();

        lastPosition = target.position;
    }

    public override void Cancel()
    {
        currentNavigateTask.Cancel();
        currentNavigateTask = null;
        base.Cancel();
    }

    public override void Update()
    {
        if (Vector3.Distance(target.position, lastPosition) > REPLAN_THRESHOLD)
        {
            currentNavigateTask.Cancel();
            currentNavigateTask = new NavigateToTask(ship, target.position, Quaternion.identity);
            currentNavigateTask.Start();

            lastPosition = target.position;
        }
        else
        {
            currentNavigateTask.Update();
        }
    }

    void OnTaskComplete()
    {
    }
}
