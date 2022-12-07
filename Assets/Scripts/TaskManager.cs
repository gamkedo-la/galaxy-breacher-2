using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    private static TaskManager _instance = null;
    public static TaskManager instance { get { return _instance; } }

    private List<Task> tasks = new List<Task>();
    private Dictionary<Task, Action> taskCompleteHandlers = new Dictionary<Task, Action>();

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
        Action onComplete = delegate() { OnTaskComplete(task); };
        task.OnComplete += onComplete;
        taskCompleteHandlers[task] = onComplete;

        tasks.Add(task);
        task.Start();
    }

    public void RemoveTask(Task task)
    {
        if (taskCompleteHandlers.ContainsKey(task))
        {
            task.OnComplete -= taskCompleteHandlers[task];
            taskCompleteHandlers.Remove(task);
        }
        tasks.Remove(task);
    }

    private void OnTaskComplete(Task task)
    {
        RemoveTask(task);
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
