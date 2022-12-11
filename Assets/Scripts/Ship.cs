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

[RequireComponent(typeof(Rigidbody), typeof(NavigationAgent))]
public class Ship : MonoBehaviour
{
    [SerializeField] private int lookaheadFrames = 20;

    [SerializeField] private float headingP = 1f;
    [SerializeField] private float headingD = 1f;
    [SerializeField] private float headingI = 1f;

    [SerializeField] private float speedP = 1f;
    [SerializeField] private float speedD = 1f;

    [SerializeField] public float maxSpeed = 40f;
    [SerializeField] public float maxAcceleration = 40f;
    [SerializeField] public float maxTurnSpeed = 40f;

    [SerializeField] public float yawAngleRange = 80;
    [SerializeField] public float pitchAngleRange = 80;
    [SerializeField] public float yawDeviationCost = 0.2f;
    [SerializeField] public float pitchDeviationCost = 0.4f;
    [SerializeField] public float avoidanceDistance = 50f;
    private float minTurnRadius;

    private new Rigidbody rigidbody;
    private Navigation.Agent agent;

    private List<Navigation.Position> path;
    private float currentMaxSpeed;

    private Navigation.Position prevPosition;

    private PIDControl headingController;
    private PIDControl speedController;
    private FloatLowPassFilter headingYawFilter = new FloatLowPassFilter(3);

    private float nextOptimizePathTime = 0f;

    void Awake()
    {
        agent = GetComponent<NavigationAgent>()?.agent;

        rigidbody = GetComponent<Rigidbody>();
        path = new List<Navigation.Position>();

        currentMaxSpeed = maxSpeed;
        UpdateMinTurnRadius();

        headingController = new PIDControl();
        headingController.Setup(headingP, headingI, headingD, 45f / Time.fixedDeltaTime);
        speedController = new PIDControl();
        speedController.Setup(speedP, 0f, speedD, 1f);
    }

    void OnValidate()
    {
        if (headingController != null)
        {
            headingController.Setup(headingP, headingI, headingD, 45f / Time.fixedDeltaTime);
        }
        if (speedController != null)
        {
            speedController.Setup(speedP, 0f, speedD, 1f);
        }

        UpdateMinTurnRadius();
    }

    void UpdateMinTurnRadius()
    {
        minTurnRadius = 360 / maxTurnSpeed * maxSpeed / 2 / Mathf.PI;
    }

    void Update()
    {
        if (path.Count > 1 && Time.time > nextOptimizePathTime)
        {
            OptimizePath();
            nextOptimizePathTime = Time.time + 1f;
        }
    }

    void OptimizePath()
    {
        while (path.Count > 1 && Navigation.Utils.IsCollisionFree(NavigationManager.instance.world, agent, prevPosition, path[1]))
        {
            path.RemoveAt(0);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = rigidbody.velocity.magnitude;
        Navigation.Position nextPosition = null;
        float progress = 0f;
        while (path.Count > 0)
        {
            nextPosition = path[0];
            progress = Mathf.Max(0f, Vector3.Dot((nextPosition.position - prevPosition.position).normalized, transform.position - prevPosition.position) / Vector3.Distance(prevPosition.position, nextPosition.position));
            if (progress < 1f)
            {
                break;
            }

            prevPosition = nextPosition;
            path.RemoveAt(0);
        }

        if (path.Count == 0)
        {
            // Path is done, stop
            currentSpeed = Mathf.Max(0f, currentSpeed - maxAcceleration * Time.fixedDeltaTime);
            rigidbody.velocity = transform.forward * currentSpeed;
            rigidbody.angularVelocity = Vector3.zero;
            return;
        }

        Vector3 currentDesiredPosition = Vector3.Lerp(prevPosition.position, nextPosition.position, progress);

        float distanceToCover = currentMaxSpeed * Time.fixedDeltaTime * lookaheadFrames;
        while (path.Count > 0 && distanceToCover > 0)
        {
            nextPosition = path[0];
            if (Mathf.Approximately(distanceToCover, 0f))
            {
                break;
            }
            float segmentDistance = Vector3.Distance(prevPosition.position, nextPosition.position);
            float nextPositionDistance = segmentDistance * (1f - progress);
            if (distanceToCover < nextPositionDistance)
            {
                progress += distanceToCover / segmentDistance;
                break;
            }

            distanceToCover -= nextPositionDistance;
            prevPosition = nextPosition;;
            path.RemoveAt(0);
        }

        Vector3 desiredPosition;
        Quaternion desiredRotation;

        if (path.Count == 0)
        {
            desiredPosition = prevPosition.position;
            desiredRotation = prevPosition.rotation;
        }
        else
        {
            desiredPosition = Vector3.Lerp(prevPosition.position, nextPosition.position, progress);

            desiredRotation = Quaternion.Slerp(prevPosition.rotation, nextPosition.rotation, progress);
            desiredRotation = Quaternion.LookRotation(nextPosition.position - prevPosition.position, desiredRotation * Vector3.up);
        }

        Vector3 lookAheadPosition = transform.position + transform.forward * currentMaxSpeed * Time.fixedDeltaTime * lookaheadFrames;

        float error = Vector3.Distance(lookAheadPosition, desiredPosition);
        float headingControl = headingController.GetControl(Time.time, error);

        if (currentDesiredPosition != transform.position)
        {
            desiredRotation = Quaternion.RotateTowards(
                desiredRotation,
                Quaternion.LookRotation(currentDesiredPosition - transform.position, desiredRotation * Vector3.up),
                headingControl * Time.fixedDeltaTime
            );
        }

        float desiredSpeed = currentMaxSpeed;

        Vector3 currentVelocity = transform.forward * currentSpeed;

        // Collect obstacles that on our course
        float timeToAvoidance = float.PositiveInfinity;

        // Debug.Log("Obstacle avoidance start");
        List<Rigidbody> obstacles = new List<Rigidbody>();
        foreach (Rigidbody body in GameObject.FindObjectsOfType<Rigidbody>())
        {
            // Debug.Log("Checking rigidbody " + body.transform.name);
            if (body == rigidbody)
                continue;

            Vector3 relativePosition = body.transform.position - transform.position;
            // Debug.Log("Obstacle relative position: " + relativePosition);
            Vector3 relativeVelocity = currentVelocity - body.velocity;
            // Debug.Log("Obstacle relative velocity: " + relativeVelocity);
            float angle = Vector3.Angle(relativePosition, relativeVelocity);
            // Debug.Log("Angle bias: " + angle);
            if (Mathf.Abs(angle) > 90f)
                continue;

            float closestDistance = relativePosition.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);
            // Debug.Log("Obstacle closest distance: " + closestDistance);
            if (closestDistance > avoidanceDistance)
                continue;

            float d = relativePosition.magnitude;
            float t = (d * Mathf.Cos(angle * Mathf.Deg2Rad) - minTurnRadius - avoidanceDistance + d * Mathf.Sin(angle * Mathf.Deg2Rad)) / currentSpeed;
            // Debug.Log("Obstacle time to avoidance: " + t);
            if (t < timeToAvoidance)
            {
                timeToAvoidance = t;
            }

            obstacles.Add(body);
        }
        // Debug.Log("Found " + obstacles.Count + " obstacles");

        // Debug.Log("Time to avoidance: " + timeToAvoidance);
        if (timeToAvoidance <= 0f)
        {
            for (int attempt=0; attempt < 3; attempt++)
            {
                float bestCost = float.PositiveInfinity;
                float bestYawAngle = 0f;
                float bestPitchAngle = 0f;

                for (int i=0; i < 10; i++)
                {
                    float yawAngle = -yawAngleRange + 2 * yawAngleRange * i / 10;
                    for (int j=0; j < 10; j++)
                    {
                        float pitchAngle = -pitchAngleRange + 2 * pitchAngleRange * j / 10;

                        Vector3 newVelocity = Quaternion.Euler(pitchAngle, yawAngle, 0f) * currentVelocity;

                        // Vector3 avoidancePosition = transform.position + newVelocity * timeToAvoidance;
                        Vector3 avoidancePosition = transform.position;

                        float minD = float.PositiveInfinity;
                        foreach (Rigidbody obstacle in obstacles)
                        {
                            // Vector3 avoidanceRelativePosition = obstacle.transform.position + obstacle.velocity * timeToAvoidance - avoidancePosition;
                            Vector3 avoidanceRelativePosition = obstacle.transform.position - avoidancePosition;
                            float d = Vector3.Cross(newVelocity, avoidanceRelativePosition).magnitude / avoidanceRelativePosition.magnitude;
                            if (d < minD)
                            {
                                minD = d;
                            }
                        }

                        if (minD >= avoidanceDistance)
                        {
                            Vector3 desiredAngles = (Quaternion.Inverse(transform.rotation) * desiredRotation).eulerAngles;
                            float cost = Mathf.Abs(yawAngle - desiredAngles.y) * yawDeviationCost + Mathf.Abs(pitchAngle - desiredAngles.x) * pitchDeviationCost;
                            if (cost < bestCost)
                            {
                                bestCost = cost;
                                bestYawAngle = yawAngle;
                                bestPitchAngle = pitchAngle;
                                headingController.ResetIntegral();
                            }
                        }
                    }
                }

                if (float.IsPositiveInfinity(bestCost))
                {
                    // Debug.Log("Couldn't find avoidance maneuer");
                    currentVelocity *= 0.7f;
                    desiredSpeed *= 0.7f;
                }
                else
                {
                    // Debug.Log("Avoidance yaw = " + bestYawAngle + ", pitch = " + bestPitchAngle);
                    desiredRotation = transform.rotation * Quaternion.Euler(bestPitchAngle, bestYawAngle, 0f);
                    break;
                }
            }
        }
        else
        {
            // float speedControl = Mathf.Max(0f, speedController.GetControl(Time.time, error));  // Speed control can only slow down, can't be negative
            // desiredSpeed = Mathf.Max(currentMaxSpeed * 0.3f, currentMaxSpeed - speedControl * currentMaxSpeed);
        }

        // Debug.Log("Position error = " + error);
        // Debug.Log("Heading control = " + headingControl);
        // Debug.Log("Speed control = " + speedControl);
        // Debug.DrawLine(lookAheadPosition, desiredPosition, Color.blue);

        Quaternion q = Quaternion.RotateTowards(transform.rotation, desiredRotation, maxTurnSpeed * Time.fixedDeltaTime);

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
        UpdateMinTurnRadius();

        Navigation.Position initial = new Navigation.Position(transform);
        Navigation.Position final = new Navigation.Position(targetPosition, targetRotation);

        Navigation.Position[] new_path = NavigationManager.instance.GetPath(agent, initial, final);

        path.Clear();
        if (new_path != null && new_path.Length >= 2)
        {
            path.AddRange(new_path);
            prevPosition = path[0];
            path.RemoveAt(0);
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
            foreach (Navigation.Position position in path)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(prevPosition, position.position);
                Gizmos.DrawSphere(position.position, 2f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(position.position, position.position + position.rotation * Vector3.forward * 5f);

                prevPosition = position.position;
            }
        }
    }
}
