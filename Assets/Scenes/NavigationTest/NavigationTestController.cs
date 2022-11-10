using UnityEngine;

public class NavigationTestController : MonoBehaviour
{
    [SerializeField] Ship[] ships;

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
            Debug.Log("Point " + p);
            leaderMovement.AddTask(new NavigateToTask(leader, p, Quaternion.AngleAxis(-i, Vector3.up), leaderSpeed));

            lastP = p;
        }
        // leaderMovement.AddTask(new NavigateToTask(leader, new Vector3(0, 0, 200f), Quaternion.AngleAxis(-45, Vector3.up), leaderSpeed));
        // leaderMovement.AddTask(new NavigateToTask(leader, new Vector3(-200, 0, 200f), Quaternion.AngleAxis(-135, Vector3.up), leaderSpeed));
        // leaderMovement.AddTask(new NavigateToTask(leader, new Vector3(-200, 0, 0f), Quaternion.AngleAxis(-225, Vector3.up), leaderSpeed));
        // leaderMovement.AddTask(new NavigateToTask(leader, new Vector3(0, 0, 0f), Quaternion.AngleAxis(-315, Vector3.up), leaderSpeed));
        RepeatTask repeat = new RepeatTask(leaderMovement);
        TaskManager.instance.AddTask(repeat);

        FormationFollowTask followTask = new FormationFollowTask(leader, new LineFormation(50f));
        for (int i=1; i < ships.Length; i++)
        {
            followTask.AddFollower(ships[i]);
        }
        TaskManager.instance.AddTask(followTask);
    }
}
