using System;
using System.Collections.Generic;
using UnityEngine;
using Navigation;

class FloatLowPassFilter
{
    float[] buffer;
    int index;
    float sum;

    public FloatLowPassFilter(int size)
    {
        buffer = new float[size];
        index = 0;
        sum = 0f;
    }

    public void Add(float value)
    {
        sum = sum - buffer[index] + value;
        buffer[index] = value;
        index = (index + 1) % buffer.Length;
    }

    public float GetValue()
    {
        return sum / buffer.Length;
    }
}

[RequireComponent(typeof(Rigidbody))]
public class Ship : MonoBehaviour
{
    [SerializeField] private float headingP = 1f;
    [SerializeField] private float headingD = 1f;

    [SerializeField] private float speedP = 1f;
    [SerializeField] private float speedD = 1f;

    [SerializeField] public float maxSpeed = 40f;
    [SerializeField] public float maxAcceleration = 40f;
    [SerializeField] public float maxTurnSpeed = 40f;

    [SerializeField] private LayerMask collisionLayers;

    public Navigation.Agent agent;
    private new Rigidbody rigidbody;

    private Queue<Navigation.State> path;
    private float currentMaxSpeed;

    private Navigation.State prevState;

    private PIDControl headingController;
    private PIDControl speedController;
    private FloatLowPassFilter headingYawFilter = new FloatLowPassFilter(3);

    void Awake()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders.Length == 0) {
            Debug.LogError("No colliders for ship " + name);
            return;
        }

        Bounds bounds = colliders[0].bounds;
        for (int i=1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        agent = new Navigation.Agent();
        agent.offset = bounds.center - transform.position;
        agent.size = bounds.size;
        agent.collisionLayers = collisionLayers;

        rigidbody = GetComponent<Rigidbody>();
        path = new Queue<Navigation.State>();

        currentMaxSpeed = maxSpeed;

        headingController = new PIDControl();
        headingController.Setup(headingP, 0f, headingD);
        speedController = new PIDControl();
        speedController.Setup(speedP, 0f, speedD);
    }

    void OnValidate()
    {
        if (headingController != null)
        {
            headingController.Setup(headingP, 0f, headingD);
        }
        if (speedController != null)
        {
            speedController.Setup(speedP, 0f, speedD);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = rigidbody.velocity.magnitude;
        Navigation.State nextState = null;
        float progress = 0f;
        while (path.Count > 0)
        {
            nextState = path.Peek();
            progress = Mathf.Max(0f, Vector3.Dot((nextState.position - prevState.position).normalized, transform.position - prevState.position) / Vector3.Distance(prevState.position, nextState.position));
            if (progress < 1f)
            {
                break;
            }

            prevState = path.Dequeue();
        }

        if (path.Count == 0)
        {
            rigidbody.angularVelocity = Vector3.zero;
            currentSpeed = Mathf.Max(0f, currentSpeed - maxAcceleration * Time.fixedDeltaTime);
            rigidbody.velocity = transform.forward * currentSpeed;
            return;
        }

        Vector3 desiredPosition = Vector3.Lerp(prevState.position, nextState.position, Mathf.Min(progress, 1f));
        Quaternion desiredRotation = Quaternion.LookRotation(nextState.position - prevState.position);

        float error = Vector3.Distance(transform.position, desiredPosition);
        float headingControl = headingController.GetControl(Time.time, error);

        Vector3 n = (desiredPosition - transform.position).normalized;
        Vector3 up = Vector3.Cross(n, transform.forward);
        desiredRotation = Quaternion.AngleAxis(headingControl, -up) * desiredRotation;

        float speedControl = speedController.GetControl(Time.time, error);
        float desiredSpeed = Mathf.Max(currentMaxSpeed * 0.1f, currentMaxSpeed - speedControl * currentMaxSpeed);

        Vector3 inward = Vector3.Cross(desiredRotation * Vector3.forward, up);

        Debug.DrawLine(transform.position, desiredPosition, Color.blue);
        Debug.DrawLine(transform.position, transform.position + desiredRotation * Vector3.forward * 5f, Color.yellow);
        Debug.DrawLine(transform.position, transform.position + up * 5f, Color.green);

        Quaternion q = Quaternion.RotateTowards(transform.rotation, desiredRotation, maxTurnSpeed * Time.fixedDeltaTime);
        Quaternion diff = Quaternion.Inverse(transform.rotation) * q;

        float turnAngle = diff.eulerAngles.y;
        while (turnAngle > 180f)
        {
            turnAngle -= 360f;
        }
        headingYawFilter.Add(turnAngle);
        turnAngle = headingYawFilter.GetValue();

        q = q * Quaternion.AngleAxis(Mathf.Clamp(turnAngle, -90f, 90f), -Vector3.forward);

        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.MoveRotation(q);

        if (currentSpeed < desiredSpeed)
        {
            currentSpeed = Mathf.Min(desiredSpeed, currentSpeed + maxAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Max(desiredSpeed, currentSpeed - maxAcceleration * Time.fixedDeltaTime);
        }
        rigidbody.velocity = transform.forward * currentSpeed;
    }

    public void NavigateTo(Vector3 targetPosition, Quaternion targetRotation, float speed)
    {
        currentMaxSpeed = speed;

        Navigation.State initial = Navigation.State.FromTransform(transform);
        Navigation.State final = new Navigation.State(targetPosition, targetRotation);

        Navigation.State[] new_path = NavigationManager.instance.GetPath(agent, initial, final);

        path.Clear();
        if (new_path != null && new_path.Length >= 2)
        {
            foreach (var state in new_path)
            {
                path.Enqueue(state);
            }
            prevState = path.Dequeue();
        }
    }

    public void Stop()
    {
        path.Clear();
    }

    void OnDrawGizmosSelected()
    {
        if (path != null && path.Count > 0)
        {
            Vector3 prevPosition = transform.position;
            foreach (Navigation.State state in path)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(prevPosition, state.position);
                Gizmos.DrawSphere(state.position, 2f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(state.position, state.position + state.orientation * Vector3.forward * 5f);

                prevPosition = state.position;
            }

        }
    }
}
