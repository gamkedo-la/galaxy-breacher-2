using UnityEngine;

public class NavigationTestController : MonoBehaviour
{
    public Ship[] ships;
    public Transform target;

    void Start()
    {
        if (ships.Length == 0)
        {
            Debug.Log("No ships configured");
            return;
        }

        Ship leader = ships[0];

        Task task = new TopLevelTask(new ChaseTask(leader, target));
        task.Start();

        FormationFollowTask followTask = new FormationFollowTask(leader, new LineFormation(50f));
        for (int i=1; i < ships.Length; i++)
        {
            followTask.AddFollower(ships[i]);
        }
        Task task2 = new TopLevelTask(followTask);
        task2.Start();
    }
}
