using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Probe : MonoBehaviour
{
    public float rotationSpeed = 20;
    Vector3 axis = Vector3.left;

    public Material on, off;
    public float timer;

    public Renderer tobeChanged;

    void Update()
    {
        transform.Rotate(axis.normalized * rotationSpeed * Time.deltaTime);

        timer += Time.deltaTime;
        if (timer >= 5 && timer <= 7)
        {
            tobeChanged.GetComponent<Renderer>().material = on;
        }
        if (timer >= 7)
        {
            tobeChanged.GetComponent<Renderer>().material = off;
            timer = 0;
        }
    }
}
