public class TopLevelTask : Task
{
    Task task;

    public TopLevelTask(Task task)
    {
        this.task = task;
    }

    public override void Start()
    {
        TaskManager.instance.AddTask(task);
    }

    public override void Cancel()
    {
        TaskManager.instance.RemoveTask(task);
    }
}
