using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    private static TaskManager _instance = null;
    public static TaskManager instance { get { return _instance; } }

    private List<Task> tasks = new List<Task>();

    void Awake()
    {
        if (_instance != null)
        {
            Debug.Log("Multiple TaskManager instances exist");
            Destroy(this);
            return;
        }

        _instance = this;
    }

    public void AddTask(Task task)
    {
        tasks.Add(task);
        task.OnComplete += () => {
            Debug.Log("Task completed: " + task);
            tasks.Remove(task);
        };
        task.Start();
    }

    public void RemoveTask(Task task)
    {
        // TODO: remove all subscriptions to task.OnComplete from this task
        tasks.Remove(task);
    }

    void Update()
    {
        // Update tasks
        foreach (Task task in new List<Task>(tasks))
        {
            task.Update();
        }
    }
}
