using UnityEngine;

public class RepeatTask : Task
{
    private Task task;

    public RepeatTask(Task task)
    {
        this.task = task;
        task.OnComplete += OnTaskComplete;
    }

    public override void Start()
    {
        base.Start();
        task.Start();
    }

    public override void Cancel()
    {
        task.Cancel();
        base.Cancel();
    }

    public override void Update()
    {
        task.Update();
    }

    void OnTaskComplete()
    {
        task.Start();
    }
}
