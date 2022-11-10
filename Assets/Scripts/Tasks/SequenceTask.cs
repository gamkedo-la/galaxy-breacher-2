using System.Collections.Generic;
using UnityEngine;

public class SequenceTask : Task
{
    private List<Task> tasks = new List<Task>();
    private int currentTask = -1;

    public SequenceTask(Task[] tasks = null)
    {
        if (tasks != null)
        {
            foreach (Task task in tasks)
            {
                AddTask(task);
            }
        }
    }

    public void AddTask(Task task)
    {
        tasks.Add(task);
        task.OnComplete += OnSubtaskComplete;
    }

    public override void Start()
    {
        if (tasks.Count == 0)
        {
            Completed();
            return;
        }

        base.Start();
        currentTask = 0;
        tasks[0].Start();
    }

    public override void Cancel()
    {
        if (currentTask < 0)
        {
            return;
        }

        tasks[currentTask].Cancel();
        base.Cancel();
    }

    public override void Update()
    {
        if (currentTask < 0)
        {
            return;
        }

        tasks[currentTask].Update();
    }

    void OnSubtaskComplete()
    {
        if (currentTask < 0)
        {
            return;
        }

        currentTask += 1;
        Debug.Log("Sequence: advancing to task " + currentTask);
        if (currentTask == tasks.Count)
        {
            currentTask = -1;
            Completed();
            return;
        }

        tasks[currentTask].Start();
    }
}
