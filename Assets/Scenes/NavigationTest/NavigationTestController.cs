using System.Collections.Generic;
using UnityEngine;

public class NavigationTestController : MonoBehaviour
{

    [SerializeField] Ship[] ships;

    List<Vector3> points = new List<Vector3>();

    void Start()
    {
        if (ships.Length == 0)
        {
            return;
        }

        Ship leader = ships[0];
        float leaderSpeed = leader.maxSpeed * 0.6f;

        SequenceTask leaderMovement = new SequenceTask();
        Vector3 lastP = leader.transform.position;
        for (int i=0; i < 360; i += 30)
        {
            float angle = Mathf.Deg2Rad * i;
            Vector3 p = new Vector3(0, 0, 200f) + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 100f;
            Debug.DrawLine(lastP, p, Color.red, 10f);
            leaderMovement.AddTask(new NavigateToTask(leader, p, Quaternion.AngleAxis(-i, Vector3.up), leaderSpeed));

            points.Add(p);

            lastP = p;
        }
        RepeatTask repeat = new RepeatTask(leaderMovement);
        TaskManager.instance.AddTask(repeat);

        FormationFollowTask followTask = new FormationFollowTask(leader, new LineFormation(50f));
        for (int i=1; i < ships.Length; i++)
        {
            followTask.AddFollower(ships[i]);
        }
        TaskManager.instance.AddTask(followTask);
    }

    void OnDrawGizmos()
    {
        if (points.Count > 0)
        {
            Gizmos.color = Color.red;
            for (int i=1; i < points.Count; i++)
            {
                Gizmos.DrawLine(points[i-1], points[i]);
            }
            Gizmos.DrawLine(points[points.Count-1], points[0]);
        }
    }
}
