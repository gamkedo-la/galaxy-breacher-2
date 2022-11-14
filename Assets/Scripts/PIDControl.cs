using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PIDControl
{
    [Tooltip("Proportional gain")]
    [SerializeField] float P;

    [Tooltip("Integral gain")]
    [SerializeField] float I;

    [Tooltip("Derivative gain")]
    [SerializeField] float D;

    private float lastTime = 0f;
    private float lastError = 0f;
    private float cumulativeError = 0f;

    public void Reset()
    {
        lastTime = 0f;
        lastError = 0f;
        cumulativeError = 0f;
    }

    public void Setup(float p, float i, float d)
    {
        this.P = p;
        this.I = i;
        this.D = d;
    }

    public float GetControl(float time, float currentError)
    {
        float control;
        if (lastTime == 0f)
        {
            control = currentError * P;
        }
        else
        {
            float deltaTime = time - lastTime;
            control = currentError * P + (currentError - lastError) / deltaTime * D + cumulativeError * I;
            cumulativeError += currentError * deltaTime;
        }

        lastError = currentError;
        lastTime = time;

        return control;
    }
}
