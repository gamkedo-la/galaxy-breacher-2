using System;
using UnityEngine;

public class Task
{
    public event Action OnComplete;

    public virtual void Start()
    {
        // TaskManager.instance.AddTask(this);
    }

    public virtual void Cancel()
    {
        // TaskManager.instance.RemoveTask(this);
    }

    public virtual void Update()
    {
    }

    protected void Completed()
    {
        OnComplete?.Invoke();
    }
}
