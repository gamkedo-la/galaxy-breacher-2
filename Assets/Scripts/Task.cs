using System;
using UnityEngine;

public class Task
{
    public event Action OnComplete;

    public virtual void Start()
    {
    }

    public virtual void Cancel()
    {
    }

    public virtual void Update()
    {
    }

    protected void Completed()
    {
        OnComplete?.Invoke();
    }
}
