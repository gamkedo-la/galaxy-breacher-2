using System;
using System.Collections.Generic;
using UnityEngine;
using Navigation;

[RequireComponent(typeof(Rigidbody))]
public class Ship : MonoBehaviour
{
    const float TARGET_REACH_DISTANCE = 5f;

    [SerializeField] public float maxSpeed = 100f;
    [SerializeField] public float maxTurnSpeed = 90f;

    [SerializeField] private LayerMask collisionLayers;

    public Navigation.Agent agent;
    private new Rigidbody rigidbody;

    private Queue<Navigation.State> path;
    private float currentMaxSpeed;

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
    }

    void FixedUpdate()
    {
        while (path.Count > 0 && Vector3.Distance(transform.position, path.Peek().position) < TARGET_REACH_DISTANCE)
        {
            path.Dequeue();
        }

        if (path.Count == 0)
        {
            return;
        }

        Navigation.State nextState = path.Peek();

        Vector3 direction = (nextState.position - transform.position).normalized;
        rigidbody.MovePosition(transform.position + direction * currentMaxSpeed * Time.fixedDeltaTime);

        Quaternion targetOrientation = nextState.orientation;

        rigidbody.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetOrientation, maxTurnSpeed));
    }

    public void NavigateTo(Vector3 targetPosition, Quaternion targetRotation, float speed)
    {
        currentMaxSpeed = speed;

        Navigation.State initial = Navigation.State.FromTransform(transform);
        Navigation.State final = new Navigation.State(targetPosition, targetRotation);

        Navigation.State[] new_path = NavigationManager.instance.GetPath(agent, initial, final);

        path.Clear();
        if (new_path != null)
        {
            foreach (var state in new_path)
            {
                path.Enqueue(state);
            }
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
