using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class NavigationPathPlannerTester : MonoBehaviour
{
    public NavigationManager navigationManager;
    public NavigationAgent agent;
    public Transform start;
    public Transform finish;

    private List<Navigation.Position> path = new List<Navigation.Position>();

    void OnValidate()
    {
        path.Clear();
        if (!IsValid)
        {
            Debug.Log("Path tester settings are not valid");
            return;
        }

        CalculatePath();
    }

    void OnEnable()
    {
        path.Clear();
        if (!IsValid)
        {
            Debug.Log("Path tester settings are not valid");
            return;
        }

        CalculatePath();
    }

    void Update()
    {
        if (!IsValid)
            return;

        if (path.Count != 0 && path[0].Matches(start) && path[path.Count-1].Matches(finish))
            return;

        CalculatePath();
    }

    bool IsValid {
        get {
            return !(
                navigationManager is null ||
                agent is null ||
                start is null ||
                finish is null
            );
        }
    }

    void CalculatePath()
    {
        path.Clear();
        path.AddRange(
            navigationManager.GetPath(
                agent.agent,
                new Navigation.Position(start),
                new Navigation.Position(finish)
            )
        );
    }

    void OnDrawGizmos()
    {
        if (path.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(path[0].position, 2f);
        for (int i=1; i < path.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(path[i].position, 2f);
            Gizmos.DrawLine(path[i-1].position, path[i].position);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(path[i].position, path[i].position + path[i].rotation * Vector3.forward * 10f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(path[i].position, path[i].position + path[i].rotation * Vector3.up * 10f);
        }
    }
}

